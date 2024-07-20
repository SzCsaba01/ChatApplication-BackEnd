namespace Project.Data.Dto.Message;
public class GetMessageDto {
    public Guid ChatId { get; set; }
    public string SenderUsername { get; set; }
    public string SenderProfilePicture { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}
