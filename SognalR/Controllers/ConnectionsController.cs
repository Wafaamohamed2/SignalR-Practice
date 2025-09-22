using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignalR.Models;
using System.Security.Claims;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConnectionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConnectionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Online")]
        public IActionResult GetOnlineUsers()
        {
            var users = _context.UserConnections
                  .Where(x => x.IsConnected)
                .Select(x => new
                {
                    x.Id,
                    x.ConnectionId,
                    x.ConnectedAt
                })
                .ToList();

            return Ok(users);
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserConnections()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var connections = _context.UserConnections
                .Where(x => x.UserId == userId && x.IsConnected)
                .Select(c => new
                {
                    c.ConnectionId,
                    c.ConnectedAt
                })
                .ToList();

            if (!connections.Any())
                return NotFound($"No active connections found for user {userId}");

            return Ok(connections);

        }
    }
}
