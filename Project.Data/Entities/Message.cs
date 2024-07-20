using System.ComponentModel.DataAnnotations;

namespace Project.Data.Entities;
public class Message {
    public Guid Id { get; set; }
    [Required]
    public Chat Chat { get; set; }
    [MaxLength(400, ErrorMessage = "Message can contain maximum 400 characters")]
    public string? Text { get; set; }
    public string? File { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public User User { get; set; }
}
