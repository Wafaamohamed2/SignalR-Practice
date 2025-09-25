using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.Models;
using SognalR.HubConfig;

namespace SignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

            var userId = request.UserId;
            var groupName = request.GroupName;

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


            // notify all group members that a new user has joined
            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification", "New Memaber ", $"{userId} Joined to {groupName}");

           // notify the user that he joined the group
            var connections = await _context.UserConnections
                .Where(uc => uc.UserId == userId && uc.IsConnected)
                .Select(uc => uc.ConnectionId)
                .ToListAsync();

            foreach (var connection in connections)
            {
                await _hubContext.Clients.Client(connection)
                           .SendAsync("ReceiveNotification", "Hello", $" {userId}, You have joined successfully to group {groupName}");
            }

            return Ok(new { message = $"User {userId} joined to group {groupName}" });
        }

        public record JoinGroupRequest(string UserId, string GroupName);

    }
}

