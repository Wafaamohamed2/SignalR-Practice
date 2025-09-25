using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SognalR.HubConfig;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("SendToGroup")]
        public async Task<IActionResult> SendMessageToGroup([FromBody] GroupMessageRequest request)
        {
            var senderName = User.Identity?.Name ?? "Anonymous";

            await _hubContext.Clients.Group(request.GroupName)
                .SendAsync("ReceiveGroupMessage", senderName, request.GroupName, request.Message);

            return Ok(new { message = $"Message sent to {request.GroupName}" });
        }

        public record GroupMessageRequest(string GroupName, string Message);
    }
}
