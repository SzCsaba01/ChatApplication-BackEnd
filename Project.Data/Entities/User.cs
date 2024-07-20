using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Project.Data.Entities;
[Serializable]
public class User {
    public Guid Id { get; set; }
    [XmlElement]
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50, ErrorMessage = "Username can have max 50 characters")]
    [MinLength(5, ErrorMessage = "Username must contain atleast 50 characters")]
    [RegularExpression("[a-zA-Z0-9._]+", ErrorMessage = "Username can contain only lower, upper cases and numbers")]
    public string Username { get; set; }
    [Required(ErrorMessage = "Email is required")]
    [MaxLength (50,  ErrorMessage = "Email can have max 50 characters")]
    [EmailAddress]
    [XmlElement]
    public string Email { get; set; }
    [Required(ErrorMessage = "First Name is required")]
    [MaxLength(50, ErrorMessage = "First Name can have max 50 characters")]
    [XmlElement]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last Name is required")]
    [MaxLength(50, ErrorMessage = "Last Name can have max 50 characters")]
    [XmlElement]
    public string LastName { get; set; }
    [XmlIgnore]
    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[-+_!@#$%^&*.,?]).+$", ErrorMessage = "Password must contain atleast one lowercase, uppercase, number and special character")]
    public string Password { get; set; }
    [Required(ErrorMessage = "Role is required")]
    [XmlIgnore]
    public Role Role { get; set; }
    [XmlIgnore]
    public ICollection<Chat>? Chats { get; set; }
    [XmlIgnore]
    public ICollection<Message>? Messages { get; set; }
    [XmlIgnore]
    public ICollection<User>? FriendList { get; set; }
    [XmlIgnore]
    public ICollection<FriendRequest>? SentFriendRequests { get; set; }
    [XmlIgnore]
    public ICollection<FriendRequest>? RecievedFriendRequest { get; set; }
    [XmlIgnore]
    public ICollection<Chat>? ChatsAdmin { get; set; }
    [XmlIgnore]
    public string? ProfilePicture { get; set; }
    [XmlIgnore]
    public string? ResetPasswordToken { get; set; }
    [XmlIgnore]
    public DateTime? ResetPasswordTokenGenerationTime { get; set; }
    [XmlIgnore]
    public string? RegistrationToken { get; set; }
    [XmlIgnore]
    public bool? IsConfirmedRegistrationToken {get; set; }
    [XmlIgnore]
    public ICollection<UserActivity>? UserActivities { get; set; }

}
