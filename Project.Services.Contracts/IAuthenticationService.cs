using Project.Data.Dto.Authentication;

namespace Project.Data.DTO.Authentication;
public interface IAuthenticationService {
    Task<AuthenticationResponseDto> LoginAsync(AuthenticationRequestDto authenticationRequestDto);
    Task LogoutAsync(Guid userId);
}