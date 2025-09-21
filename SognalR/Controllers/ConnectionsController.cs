using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignalR.Models;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult GetUserConnections(string userId) {

            var connections = _context.UserConnections
                .Where(x => x.UserId == userId)
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
