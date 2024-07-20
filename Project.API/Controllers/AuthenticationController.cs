using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Data.Dto.Authentication;
using Project.Data.DTO.Authentication;

namespace Project.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuthenticationController : ControllerBase {
    private readonly IAuthenticationService _authenticationService;
    public AuthenticationController(IAuthenticationService authenticationService) {
        _authenticationService = authenticationService;
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync(AuthenticationRequestDto authenticationRequestDto) {
        return Ok(await _authenticationService.LoginAsync(authenticationRequestDto));
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> LogoutAsync(Guid id) {
        await _authenticationService.LogoutAsync(id);
        return Ok();
    }
}

