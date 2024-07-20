using Project.Data.Entities;

namespace Project.Data.Dto.FriendRequest;
public class GetFriendRequestDto {
    public string SenderName { get; set; }
    public string SenderProfilePicture { get; set; }
    public FriendRequestFlag FriendRequestFlag { get; set; }
    public DateTime? Date { get; set; }
}
