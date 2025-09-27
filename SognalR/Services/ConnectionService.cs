using SignalR.Models;

namespace SignalR.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly AppDbContext _context;

        public ConnectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ConnectAsync(string userId, string connectionId)
        {
            // Mark existing connections as disconnected
            var existingConnection = _context.UserConnections
                   .FirstOrDefault(c => c.UserId == userId);

            if (existingConnection != null)
            {
                existingConnection.ConnectionId = connectionId;
                existingConnection.IsConnected = true;
                existingConnection.ConnectedAt = DateTime.UtcNow;
                existingConnection.DisconnectedAt = null;
                _context.UserConnections.Update(existingConnection);
            }

            else
            {
                _context.UserConnections.Add(new UserConnection
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ConnectionId = connectionId,
                    ConnectedAt = DateTime.UtcNow,
                    IsConnected = true
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DisconnectAsync(string connectionId)
        {
            var connection = _context.UserConnections
                .FirstOrDefault(c => c.ConnectionId == connectionId && c.IsConnected);

            if (connection != null)
            {
                connection.IsConnected = false;
                connection.DisconnectedAt = DateTime.UtcNow;
                _context.UserConnections.Update(connection);
                await _context.SaveChangesAsync();
            }
        }

        public List<UserConnection> GetOnlineUsers()
        {
            return _context.UserConnections
                .Where(c => c.IsConnected)
                .ToList();
        }

        public List<UserConnection> GetUserConnections(string userId)
        {
            return _context.UserConnections
                .Where(c => c.UserId == userId && c.IsConnected)
                .ToList();
        }
    }
}
