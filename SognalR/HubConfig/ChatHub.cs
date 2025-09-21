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

        // Register user connection and add to groups for reconnection
        public async Task RegisterUser(string userId)
        {
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

        public async Task SendMessage(string name, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", name, message);
        }

        public async Task  JoinGroup(string gName, string name)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gName);

                var connection = _context.UserConnections
                    .FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

                if (connection != null)
                {
                    var group = _context.Groups.FirstOrDefault(g => g.Name == gName);
                    if (group == null)
                    {
                        group = new Group
                        {
                            Id = Guid.NewGuid(),
                            Name = gName

                        };
                        _context.Groups.Add(group);
                        await _context.SaveChangesAsync();
                    }
                    var existingMembership = _context.UserGroups
                       .FirstOrDefault(ug => ug.UserId == connection.UserId && ug.GroupId == group.Id);

                    if (existingMembership == null)
                    {
                        var userGroup = new UserGroup
                        {
                            GroupId = group.Id,
                            UserId = connection.UserId

                        };
                        _context.UserGroups.Add(userGroup);
                       
                    }
                    await _context.SaveChangesAsync();
                }



                await Clients.OthersInGroup(gName).SendAsync("ShowWhoJoin", name, gName);
            }         
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error joining group: {ex.Message}");
                throw; // Optionally rethrow or handle the exception as needed
            }

        }

        public async Task SendMessageToGroup(string gName, string name, string message)
        {
            await Clients.Group(gName).SendAsync("ReceiveGroupMessage", name, gName,message);
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
