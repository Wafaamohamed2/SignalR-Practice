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
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public GroupsController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("Join")]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var groupName = request.GroupName;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var group = _context.Groups.FirstOrDefault(g => g.Name == groupName);
            if (group == null)
            {
                group = new Group
                {
                    Id = Guid.NewGuid(),
                    Name = groupName
                };
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();
            }

            var existingMembership = _context.UserGroups
                .FirstOrDefault(ug => ug.UserId == userId && ug.GroupId == group.Id);

            if (existingMembership != null)
            {
                return BadRequest(new { message = $"User already in group {groupName}" });
            }

            var userGroup = new UserGroup { GroupId = group.Id, UserId = userId };
            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            // notify the user that he joined the group
            var connections = await _context.UserConnections
                .Where(uc => uc.UserId == userId && uc.IsConnected)
                .Select(uc => uc.ConnectionId)
                .ToListAsync();

            foreach (var connectionId in connections)
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
                await _hubContext.Clients.Client(connectionId)
                           .SendAsync("ReceiveNotification", "Hello", $" {userId}, You have joined successfully to group {groupName}");
            }

            // notify all group members that a new user has joined
            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification", "New Member", $"{userId} Joined to {groupName}");

            return Ok(new { message = $"User {userId} joined to group {groupName}" });
        }

        public record JoinGroupRequest(string GroupName);
    }
}
