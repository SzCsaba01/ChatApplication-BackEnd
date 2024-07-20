using System.ComponentModel.DataAnnotations;

namespace Project.Data.Entities;
public class UserActivity {
    public Guid Id { get; set; }
    [Required]
    public User User { get; set; }
    [Required]
    public DateTime LoggedIn { get; set; }
    public DateTime? LoggedOut { get; set; }
}
