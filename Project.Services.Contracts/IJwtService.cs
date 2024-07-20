using Project.Data.Dto.Authentication;
using Project.Data.Dto.User;

namespace Project.Data.DTO.Authentication;
public interface IJwtService {
    Task<string> GetAuthentificationJwtAsync(GetDetailsForLoginDto user);
    Task<string> GenerateJwtTokenAsync(GenerateJwtDto generateJwtDto);
}
