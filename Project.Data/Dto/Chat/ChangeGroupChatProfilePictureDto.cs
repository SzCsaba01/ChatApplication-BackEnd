using Microsoft.AspNetCore.Http;

namespace Project.Data.Dto.Chat;
public class ChangeGroupChatProfilePictureDto {
    public Guid ChatId { get; set; }
    public IFormFile File { get; set; }
}
