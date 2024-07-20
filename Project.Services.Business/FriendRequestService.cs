using Microsoft.EntityFrameworkCore;
using Project.Data.Data;
using Project.Data.Dto.Chat;
using Project.Data.Dto.FriendRequest;
using Project.Data.Entities;
using Project.Services.Business.Exceptions;
using Project.Services.Contracts;
using SendGrid.Helpers.Errors.Model;
using System.ComponentModel.DataAnnotations;

namespace Project.Services.Business;
public class FriendRequestService : IFriendRequestService {
    private readonly DataContext _dataContext;
    private readonly IChatService _chatService;
    public FriendRequestService(DataContext dataContext, IChatService chatService) {
        _dataContext = dataContext;
        _chatService = chatService;
    }
    public async Task DeclineFriendRequestAsync(AcceptOrDeleteFriendRequestDto friendRequestDto) {
        var user = await _dataContext.Users.FindAsync(friendRequestDto.Id);

        if (user is null) {
            throw new ModelNotFoundException("User not found");
        }

        var sender = await _dataContext.Users
            .Where(u => u.Username.Equals(friendRequestDto.Username))
            .FirstOrDefaultAsync();
        
        if (sender is null) {
            throw new ModelNotFoundException("Sender not found");
        }

        var friendRequest = await _dataContext.FriendRequests
            .Where(f => f.Receiver.Id.Equals(friendRequestDto.Id))
            .Where(f => f.Sender.Username.Equals(friendRequestDto.Username))
            .FirstOrDefaultAsync();

        if (friendRequest is null) {
            throw new ModelNotFoundException("Friend request not found");
        }

        friendRequest.FriendRequestFlag = FriendRequestFlag.Deleted;
        await _dataContext.SaveChangesAsync();
    }

    public async Task AcceptFriendRequestAsync(AcceptOrDeleteFriendRequestDto friendRequestDto) {

        var user = await _dataContext.Users
            .Include(u => u.FriendList)
            .FirstOrDefaultAsync(u => u.Id == friendRequestDto.Id);

        if (user is null) {
            throw new ModelNotFoundException("User not found");
        }

        var sender = await _dataContext.Users
            .Where(u => u.Username.Equals(friendRequestDto.Username))
            .Include(u => u.FriendList)
            .FirstOrDefaultAsync();

        if (sender is null) {
            throw new ModelNotFoundException("Sender not found");
        }

        var friendRequest = await _dataContext.FriendRequests
            .Where(f => f.Receiver.Id.Equals(friendRequestDto.Id))
            .Where(f => f.Sender.Username.Equals(friendRequestDto.Username))
            .FirstOrDefaultAsync();

        if (friendRequest is null) {
            throw new ModelNotFoundException("Friend request not found");
        }

        friendRequest.FriendRequestFlag= FriendRequestFlag.Approved;

        user.FriendList.Add(sender);
        sender.FriendList.Add(user);

        var createChatDto = new CreateChatDto {
            Username1 = user.Username,
            Username2 = sender.Username,
        };

        await _chatService.CreateChatAsync(createChatDto);

        await _dataContext.SaveChangesAsync();
    }

    public async Task<GetFriendRequestsDto> GetReceivedFriendRequestsByIdAsync(Guid Id){
        var allReceivedFriendRequests = await _dataContext.FriendRequests
            .Include(f => f.Receiver)
            .Include(f => f.Sender)
            .Where(u => u.Receiver.Id.Equals(Id) && u.FriendRequestFlag == FriendRequestFlag.Sent)
            .Select(f => new GetFriendRequestDto {
                SenderName = f.Sender.Username,
                SenderProfilePicture = f.Sender.ProfilePicture,
                FriendRequestFlag = f.FriendRequestFlag,
                Date = f.CreatedAt,
            })
            .ToListAsync();

        var getFriendRequestsDto = new GetFriendRequestsDto {
            FriendRequests = allReceivedFriendRequests,
        };

        return getFriendRequestsDto;
    }

    public async Task SendFriendRequestAsync(SendFriendRequestDto friendRequestDto)
    {
        var sender = await _dataContext.Users.FindAsync(friendRequestDto.SenderId);

        if(sender is null) {
            throw new ModelNotFoundException("Sender not found");
        }

        var receiver = await _dataContext.Users
            .Where(u => u.Username.Equals(friendRequestDto.ReceiverName))
            .Include(u => u.RecievedFriendRequest)
            .ThenInclude(f => f.Sender)
            .FirstOrDefaultAsync();

        if(receiver is null) {
            throw new ModelNotFoundException("Receiver not found");
        }

        var friendRequestExists = receiver.RecievedFriendRequest
            .Where(u => u.Sender.Id == friendRequestDto.SenderId)
            .FirstOrDefault();

        if(friendRequestExists is not null) {
            friendRequestExists.FriendRequestFlag = FriendRequestFlag.Sent;
            await _dataContext.SaveChangesAsync();
            return;
        }

        var friendRequest = new FriendRequest {
            Sender = sender,
            Receiver = receiver,
            CreatedAt = DateTime.UtcNow,
            FriendRequestFlag = FriendRequestFlag.Sent,
        };

        if (receiver.RecievedFriendRequest is not null && receiver.RecievedFriendRequest.Any(f => f.Sender.Id.Equals(sender.Id))){
            throw new AlreadyExistsException("Friend request already sent");
        }

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(friendRequest, new ValidationContext(friendRequest), results, validateAllProperties: true);
        var errorMessages = results.Select(x => x.ErrorMessage);

        if (!valid) {
            throw new BadRequestException(string.Join(" ", errorMessages));
        }

        await _dataContext.FriendRequests.AddAsync(friendRequest);

        sender.SentFriendRequests.Add(friendRequest);
        receiver.RecievedFriendRequest.Add(friendRequest);

        await _dataContext.SaveChangesAsync();
    }
}
