using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Data.Dto.Chat;
using Project.Services.Contracts;

namespace Project.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase {
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService) {
        _chatService = chatService;
    }

    [Authorize(Roles ="Admin, User")]
    [HttpGet("GetChatsByUserId")]
    public async Task<IActionResult> GetChatsByUserIdAsync(Guid Id) {
        return Ok(await _chatService.GetChatsByUserIdAsync(Id));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost("LeaveChat")]
    public async Task<IActionResult> LeaveChatAsync(LeaveChatDto leaveChatDto) {
        await _chatService.LeaveChatAsync(leaveChatDto);
        return Ok("You have successfully left the chat");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost("CreateGroupChat")]
    public async Task<IActionResult> CreateGroupChatAsync([FromForm] CreateGroupChatDto createGroupChatDto) {
        await _chatService.CreatGroupChatAsync(createGroupChatDto);
        return Ok("Group chat created");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("EditGroupChat")]
    public async Task<IActionResult> EditGroupChatAsync(EditGroupChatDto editGroupChatDto) {
        await _chatService.EditGroupChatAsync(editGroupChatDto);
        return Ok("Group chat edited");
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPut("ChangeGroupChatProfilePicture")]
    public async Task<IActionResult> ChangeGroupChatProfilePictureAsync([FromForm] ChangeGroupChatProfilePictureDto changeGroupChatProfilePictureDto) {
        await _chatService.ChangeGroupChatProfilePictureAsync(changeGroupChatProfilePictureDto);
        return Ok("Group chat profile picture changed");
    }
}
