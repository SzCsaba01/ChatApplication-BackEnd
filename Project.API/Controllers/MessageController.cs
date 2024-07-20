using Microsoft.AspNetCore.Mvc;
using Project.Data.Dto.Message;
using Project.Services.Business;
using Project.Services.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Project.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase {
    private readonly IMessageService _messageService;
    private readonly IHubContext<MessageService> _hubContext;

    public MessageController(IMessageService messageService, IHubContext<MessageService> hubContext) {
        _messageService = messageService;
        _hubContext = hubContext;
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessageAsync(SendMessageDto sendMessageDto) {
        var messages = await _messageService.SendMessageAsync(sendMessageDto);
        await _hubContext.Clients.Group(sendMessageDto.ChatId.ToString()).SendAsync("ReceiveMessage", messages);

        return Ok();
    }

    [HttpGet("GetMessages")]
    public async Task<IActionResult> GetMessagesByChatIdAsync(Guid chatId) {
        return Ok(await _messageService.GetMessagesByChatIdAsync(chatId));
    }

}
