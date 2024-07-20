namespace Project.Data.Dto.Message;
public class SendMessageDto {
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string Text { get; set; }
}
