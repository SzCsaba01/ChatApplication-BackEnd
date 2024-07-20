namespace Project.Data.Dto.User;
public class UserSearchDto {
    public Guid SearcherId { get; set; }
    public string Search { get; set; }
    public int Page { get; set; }
}
