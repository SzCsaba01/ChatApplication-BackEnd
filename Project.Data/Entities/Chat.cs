using Project.Data.Dto.User;
using System.ComponentModel.DataAnnotations;

namespace Project.Data.Entities;
public class Chat {
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ChatPicture { get; set; }
    public ICollection<User>? Users { get; set; }
    [Required]
    public ICollection<User> ChatAdmins { get; set; }
    public ICollection<Message>? Messages { get; set; }
}
