using Project.Data.Dto.Message;
using Project.Data.Dto.User;

namespace Project.Data.Dto.Chat;
public class GetChatDto {
    public Guid ChatId { get; set; }
    public GetUserDto User { get; set; }
    public string? ChatName { get; set; }
    public string? ChatPicture { get; set; }
    public List<GetUserDto> Users { get; set; }
    public List<GetUserDto> ChatAdmins { get; set; }
}
