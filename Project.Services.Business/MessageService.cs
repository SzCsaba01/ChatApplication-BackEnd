using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Data.Data;
using Project.Data.Dto.Message;
using Project.Data.Entities;
using Project.Services.Business.Exceptions;
using Project.Services.Contracts;


namespace Project.Services.Business;
public class MessageService : Hub, IMessageService {
    private readonly DataContext _dataContext;
    public MessageService(DataContext dataContext) {
        _dataContext = dataContext;
    }

    public override async Task OnConnectedAsync() {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception) {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroupSocket(Guid chatId) {
        string groupId = chatId.ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task<GetMessagesDto> GetMessagesByChatIdAsync(Guid chatId) {
        var messages = await _dataContext.Chats
            .Where(c => c.Id == chatId)
            .Include(c => c.Messages)
            .ThenInclude(m => m.User)
            .Select(m => new GetMessagesDto {
                Messages = m.Messages.Select(m => new GetMessageDto {
                    Text = m.Text,
                    CreatedAt = m.CreatedAt,
                    SenderUsername = m.User.Username,
                    SenderProfilePicture = Path.Combine(AppConstants.APP_URL, m.User.ProfilePicture ?? ""),
                    ChatId = chatId,
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (messages == null) {
            throw new ModelNotFoundException("Chat not found");
        }

        return messages;
    }

    public async Task LeaveGroupSocket(Guid chatId) {
        string groupId = chatId.ToString();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task<GetMessagesDto> SendMessageAsync(SendMessageDto sendMessageDto) {
        var sender = await _dataContext.Users.FindAsync(sendMessageDto.SenderId);

        if(sender is null) {
            throw new ModelNotFoundException("User not found");
        }

        var chat = await _dataContext.Chats
            .Where(c => c.Id == sendMessageDto.ChatId)
            .Include(c => c.Users)
            .Include(c => c.ChatAdmins)
            .Include(c => c.Messages)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync();

        if (chat is null) {
            throw new ModelNotFoundException("Chat not found");
        }

        if(!chat.Users.Contains(sender) && !chat.ChatAdmins.Contains(sender)){
            throw new ModelNotFoundException("User not found in chat");
        }

        var message = new Message {
            Chat = chat,
            Text = sendMessageDto.Text,
            CreatedAt = DateTime.Now,
            User = sender,
        };

        await _dataContext.Messages.AddAsync(message);

        chat.Messages.Add(message);

        var messages = chat.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new GetMessageDto {
                SenderUsername = m.User.Username,
                Text = m.Text,
                CreatedAt = m.CreatedAt,
                SenderProfilePicture = Path.Combine(AppConstants.APP_URL, m.User.ProfilePicture ?? ""),
                ChatId = chat.Id,
            })
            .ToList();

        var getMessagesDto = new GetMessagesDto { Messages = messages };

        await _dataContext.SaveChangesAsync();

        return getMessagesDto;
    }
}
