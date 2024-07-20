using Project.Data.Dto.Message;

namespace Project.Services.Contracts;
public interface IMessageService {
    public Task<GetMessagesDto> SendMessageAsync(SendMessageDto sendMessageDto);
    public Task<GetMessagesDto> GetMessagesByChatIdAsync(Guid chatId);
    public Task LeaveGroupSocket(Guid chatId);
    public Task JoinGroupSocket(Guid chatId);
}
