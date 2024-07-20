using Project.Data.Dto.FriendRequest;

namespace Project.Services.Contracts;
public interface IFriendRequestService {
    public Task<GetFriendRequestsDto> GetReceivedFriendRequestsByIdAsync(Guid Id);
    public Task SendFriendRequestAsync(SendFriendRequestDto FriendRequest);
    public Task DeclineFriendRequestAsync(AcceptOrDeleteFriendRequestDto FriendRequestDto);
    public Task AcceptFriendRequestAsync(AcceptOrDeleteFriendRequestDto FriendRequestDto);
}
