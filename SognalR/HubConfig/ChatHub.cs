using Microsoft.AspNetCore.SignalR;
using SignalR.Models;
using System.Security.Claims;

namespace SognalR.HubConfig
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string name, string message)
        {
            // dbsave

            await Clients.All.SendAsync("ReceiveMessage", name, message);
        }

        public async Task  JoinGroup(string gName, string name)
        {
           await Groups.AddToGroupAsync(Context.ConnectionId, gName);

           await Clients.OthersInGroup(gName).SendAsync("ShowWhoJoin",name,gName);
        }

        public async Task SendMessageToGroup(string gName, string name, string message)
        {
            await Clients.Group(gName).SendAsync("ReceiveGroupMessage", name, gName,message);
        }

      

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value?? Context.ConnectionId;
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
