namespace Project.Data.Dto.Chat;
public class EditGroupChatDto {
    public Guid UserId { get; set; }
    public Guid ChatId { get; set; }
    public string ChatName { get; set; }
    public ICollection<string> UsersToBeAdded { get; set; }
    public ICollection<string> AdminsToBeAdded { get; set; }
    public ICollection<string> MembersToBeRemoved { get; set; }
}
