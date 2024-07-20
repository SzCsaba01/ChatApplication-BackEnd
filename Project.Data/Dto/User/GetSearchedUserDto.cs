using Project.Data.Entities;

namespace Project.Data.Dto.User;
public class GetSearchedUserDto {
    public string? ProfilePicture { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public FriendRequestFlag FriendRequestFlag { get; set; }
}
