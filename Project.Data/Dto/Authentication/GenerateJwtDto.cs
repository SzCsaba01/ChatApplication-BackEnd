using System.Security.Claims;

namespace Project.Data.Dto.Authentication;
public class GenerateJwtDto
{
    public ClaimsIdentity Claims { get; set; }
    public DateTime ExpirationDate { get; set; }
}
