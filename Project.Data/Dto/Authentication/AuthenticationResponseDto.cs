using Project.Data.Entities;

namespace Project.Data.Dto.Authentication;
public class AuthenticationResponseDto
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public RoleTypes Role { get; set; }
}
