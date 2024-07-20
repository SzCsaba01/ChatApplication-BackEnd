using Microsoft.EntityFrameworkCore;
using Project.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace Project.Data.Data;
public class DataContext : DbContext{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        modelBuilder.Entity<Chat>()
            .HasMany(c => c.ChatAdmins)
            .WithMany(u => u.ChatsAdmin)
            .UsingEntity("ChatAdmin");

        modelBuilder.Entity<Chat>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chats)
            .UsingEntity("ChatUser");
            

        modelBuilder.Entity<FriendRequest>()
            .HasOne(e => e.Sender)
            .WithMany(u => u.SentFriendRequests)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendRequest>()
            .HasOne(e => e.Receiver)
            .WithMany(u => u.RecievedFriendRequest)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
}
