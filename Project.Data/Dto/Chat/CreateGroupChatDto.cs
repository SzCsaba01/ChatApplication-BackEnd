using Microsoft.AspNetCore.Http;
using Project.Data.Dto.User;

namespace Project.Data.Dto.Chat;
public class CreateGroupChatDto {
    public Guid UserId { get; set; }
    public string ChatName { get; set; }
    public IFormFile File { get; set; }
    public List<string> Usernames { get; set; }
}
