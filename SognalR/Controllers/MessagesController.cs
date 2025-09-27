using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.Models;
using SognalR.HubConfig;
using System.Security.Claims;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly AppDbContext _context;

        public MessagesController(IHubContext<ChatHub> hubContext, AppDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        [HttpPost("SendToAll")]
        public async Task<IActionResult> SendMessage([FromBody] BroadcastMessageRequest request)
        {
            var senderName = User.Identity?.Name ?? "System";

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", senderName, request.Message);

            return Ok(new { message = "Message broadcasted successfully" });
        }

        [HttpPost("SendToGroup")]
        public async Task<IActionResult> SendMessageToGroup([FromBody] GroupMessageRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Check if user is a member of the group
            var group = _context.Groups.FirstOrDefault(g => g.Name == request.GroupName);
            if (group == null)
                return NotFound(new { message = $"Group {request.GroupName} does not exist" });

            var isMember = _context.UserGroups.Any(ug => ug.UserId == userId && ug.GroupId == group.Id);
            if (!isMember)
                return BadRequest(new { message = "You are not a member of this group" });

            var senderName = User.Identity?.Name ?? "Anonymous";

            await _hubContext.Clients.Group(request.GroupName)
                .SendAsync("ReceiveGroupMessage", senderName, request.GroupName, request.Message);

            return Ok(new { message = $"Message sent to group {request.GroupName}" });
        }

        public record GroupMessageRequest(string GroupName, string Message);
        public record BroadcastMessageRequest(string Message);
    }
}
