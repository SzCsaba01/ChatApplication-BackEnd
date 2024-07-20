using Microsoft.EntityFrameworkCore;
using Project.Data.Data;
using Project.Data.Dto.Authentication;
using Project.Data.Dto.User;
using Project.Data.DTO.Authentication;
using Project.Services.Contracts;
using System.Security.Authentication;

namespace Project.Services.Business;
public class AuthenticationService : IAuthenticationService {
    private readonly DataContext _dataContext;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtService _jwtService;
    private readonly IUserActivityService _userActivityService;
    public AuthenticationService(DataContext dataContext, IEncryptionService encryptionService, IJwtService jwtService, IUserActivityService userActivityService) { 
        _dataContext = dataContext;
        _encryptionService = encryptionService;
        _jwtService = jwtService;
        _userActivityService = userActivityService;
    }
    public async Task<AuthenticationResponseDto> LoginAsync(AuthenticationRequestDto authenticationRequestDto) {
        authenticationRequestDto.Password = _encryptionService.GeneratedHashedPassword(authenticationRequestDto.Password);

        var user = await _dataContext.Users
           .Where(u => (u.Username.Equals(authenticationRequestDto.UserCredentials) || u.Email.Equals(authenticationRequestDto.UserCredentials)) && u.Password.Equals(authenticationRequestDto.Password))
           .Select(u => new GetDetailsForLoginDto {
               Username = u.Username,
               Id = u.Id,
               Role = u.Role.RoleType,
               IsConfirmedRegistrationToken = (bool)u.IsConfirmedRegistrationToken,
           })
           .FirstOrDefaultAsync();

        if (user is null) {
            throw new AuthenticationException("Username or Password is incorrect");
        }

        if (!(bool)user.IsConfirmedRegistrationToken) {
            throw new AuthenticationException("Email is not verified");
        }

        await _userActivityService.UserLoginAsync(user.Id);

        var token = await _jwtService.GetAuthentificationJwtAsync(user);
        return new AuthenticationResponseDto { Id = user.Id, Role = user.Role, Token = token };
    }

    public async Task LogoutAsync(Guid userId) {
        await _userActivityService.UserLogoutAsync(userId);
    }
}
