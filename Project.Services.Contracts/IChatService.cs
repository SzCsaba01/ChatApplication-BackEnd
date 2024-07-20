using Project.Data.Dto.Chat;

namespace Project.Services.Contracts;
public interface IChatService {
    public Task CreateChatAsync(CreateChatDto createChatDto);
    public Task<GetChatsDto> GetChatsByUserIdAsync(Guid UserId);
    public Task LeaveChatAsync(LeaveChatDto leaveChatDto);
    public Task CreatGroupChatAsync(CreateGroupChatDto createGroupChatDto);
    public Task EditGroupChatAsync(EditGroupChatDto editGroupChatDto);
    public Task ChangeGroupChatProfilePictureAsync(ChangeGroupChatProfilePictureDto changeGroupChatProfilePictureDto);

}
