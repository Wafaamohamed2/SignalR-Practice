using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalR.Models;
using SognalR.HubConfig;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationsController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            var connections = _context.UserConnections
                .Where(c => c.UserId == request.UserId && c.IsConnected)
                .Select(c => c.ConnectionId)
                .ToList();

            if (!connections.Any())
                return NotFound(new { message = "User is not connected" });

            foreach (var connId in connections)
            {
                await _hubContext.Clients.Client(connId)
                    .SendAsync("ReceiveNotification", request.Title, request.Notification);
            }

            return Ok(new { message = $"Notification sent to user " });
        }

        public record SendNotificationRequest(string UserId, string Title, string Notification);
    }
}
