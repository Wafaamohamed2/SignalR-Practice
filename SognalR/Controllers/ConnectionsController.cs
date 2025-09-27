using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalR.Models;
using SignalR.Services;
using SognalR.HubConfig;
using System.Security.Claims;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConnectionsController : ControllerBase
    {
        private readonly IConnectionService _connectionService;

        public ConnectionsController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpGet("Online")]
        public IActionResult GetOnlineUsers()
        {
            var users = _connectionService.GetOnlineUsers();
            return Ok(users);
        }

        [HttpGet("MyConnections")]
        public IActionResult GetUserConnections()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var connections = _connectionService.GetUserConnections(userId);
            return Ok(connections);

        }

    
    }
}