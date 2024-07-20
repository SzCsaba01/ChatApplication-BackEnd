using System.ComponentModel.DataAnnotations;

namespace Project.Data.Entities;
public enum FriendRequestFlag {
    None,
    Sent,
    Approved,
    Deleted,
};
public class FriendRequest {
    public Guid Id { get; set; }
    [Required]
    public User Sender { get; set; }
    [Required]
    public User Receiver { get; set; }
    [Required]
    public FriendRequestFlag FriendRequestFlag { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}
