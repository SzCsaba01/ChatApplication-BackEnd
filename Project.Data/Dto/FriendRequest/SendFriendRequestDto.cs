namespace Project.Data.Dto.FriendRequest;
public class SendFriendRequestDto {
    public Guid SenderId { get; set; }
    public string ReceiverName { get; set; }
    public DateTime CreatedAt { get; set; }
}
