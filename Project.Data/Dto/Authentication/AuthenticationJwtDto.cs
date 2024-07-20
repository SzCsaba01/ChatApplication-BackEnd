using Project.Data.Entities;

namespace Project.Data.Dto.Authentication;
public class AuthenticationJwtDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public RoleTypes Role { get; set; }
}
