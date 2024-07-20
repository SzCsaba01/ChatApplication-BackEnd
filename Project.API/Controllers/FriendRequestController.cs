using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Data.Dto.FriendRequest;
using Project.Services.Contracts;

namespace Project.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FriendRequestController : ControllerBase {
    private readonly IFriendRequestService _friendRequestService;
    public FriendRequestController(IFriendRequestService friendRequestService) {
        _friendRequestService = friendRequestService;
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("GetReceivedFriendRequestsById")]
    public async Task<IActionResult> GetReceivedFriendRequestsByIdAsync(Guid Id) {
        return Ok(await _friendRequestService.GetReceivedFriendRequestsByIdAsync(Id));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost("SendFriendRequest")]
    public async Task<IActionResult> SendFriendRequestAsync(SendFriendRequestDto sendFriendRequestDto) {
        await _friendRequestService.SendFriendRequestAsync(sendFriendRequestDto);
        return Ok();
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("DeclineFriendRequest")]
    public async Task<IActionResult> DeleteFriendRequestByIdAsync(AcceptOrDeleteFriendRequestDto FriendRequest) {
        await _friendRequestService.DeclineFriendRequestAsync(FriendRequest);
        return Ok();
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("AcceptFriendRequest")]
    public async Task<IActionResult> AcceptFriendRequestAsync(AcceptOrDeleteFriendRequestDto FriendRequest) {
        await _friendRequestService.AcceptFriendRequestAsync(FriendRequest);
        return Ok();
    }
}
    