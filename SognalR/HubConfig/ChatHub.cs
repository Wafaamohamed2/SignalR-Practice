using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalR.Models;
using System.Security.Claims;

namespace SognalR.HubConfig
{
    [Authorize] // Ensure only authenticated users can access the hub
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
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
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connection = _context.UserConnections
                .FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            if (connection != null)
            {
                connection.IsConnected = false;
                connection.DisconnectedAt = DateTime.UtcNow;

                _context.UserConnections.Update(connection);
                await _context.SaveChangesAsync();
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
