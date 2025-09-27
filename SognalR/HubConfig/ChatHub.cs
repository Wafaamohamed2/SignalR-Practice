using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.Models;
using SignalR.Services;
using System.Security.Claims;

namespace SognalR.HubConfig
{
    [Authorize] // Ensure only authenticated users can access the hub
    public class ChatHub : Hub
    {
        private readonly IConnectionService _connectionService;
        private readonly AppDbContext _context;

        public ChatHub(IConnectionService connectionService, AppDbContext context)
        {
            _connectionService = connectionService;
            _context = context;
        }



        // Register user connection and add to groups for reconnection
        public async Task RegisterUser()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                throw new Exception("User not authenticated");
            }

            // Mark existing connections as disconnected
            var existingConnections = _context.UserConnections
                .Where(c => c.UserId == userId && c.IsConnected);

            foreach (var conn in existingConnections)
            {
                conn.IsConnected = false;
                conn.DisconnectedAt = DateTime.UtcNow;
            }

            // Add new connection
            var connection = new UserConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow,
                IsConnected = true
            };
            _context.UserConnections.Add(connection);
            await _context.SaveChangesAsync();


            // Get user groups
            var groups = _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Group.Name)
                .ToList();

            // Add user to their groups
            foreach (var groupName in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
        }

        public async Task SendMessage( string message)
        {
            var name = Context.User?.Identity?.Name ?? "Unknown";
            await Clients.All.SendAsync("ReceiveMessage", name, message);
        }

    
        public async Task SendNotification(string userId, string title, string notification)
        {
            var connections = _context.UserConnections
                .Where(c => c.UserId == userId && c.IsConnected)
                .Select(c => c.ConnectionId)
                .ToList();

            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveNotification", title, notification);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (!string.IsNullOrEmpty(userId))
            {
                await _connectionService.ConnectAsync(userId, Context.ConnectionId);


                // Add user to all their existing groups
                var userGroups = await _context.UserGroups
                    .Include(ug => ug.Group)
                    .Where(ug => ug.UserId == userId)
                    .Select(ug => ug.Group.Name)
                    .ToListAsync();

                foreach (var groupName in userGroups)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }

                await Clients.Caller.SendAsync("ReceiveNotification", "System", "Connected successfully");

            }
            await base.OnConnectedAsync();

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _connectionService.DisconnectAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

    }
}
