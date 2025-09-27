using SignalR.Models;

namespace SignalR.Services
{
    public interface IConnectionService
    {
        Task ConnectAsync(string userId, string connectionId);
        Task DisconnectAsync(string connectionId);
        List<UserConnection> GetOnlineUsers();
        List<UserConnection> GetUserConnections(string userId);
    }

}
