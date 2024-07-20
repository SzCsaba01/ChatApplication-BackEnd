using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Services.Contracts;

namespace Project.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserActivityController : ControllerBase {
    private readonly IUserActivityService _userActivityService;

    public UserActivityController(IUserActivityService userActivityService) {
        _userActivityService = userActivityService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("GetUserActivitiesByUsername")]
    public async Task<IActionResult> GetUserActivitiesByUsernameAsync(string username) {
        return Ok(await _userActivityService.GetUserActivitiesByUsernameAsync(username));
    }

}
