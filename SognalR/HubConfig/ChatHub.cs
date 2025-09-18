using Microsoft.AspNetCore.SignalR;

namespace SognalR.HubConfig
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string name, string message)
        {
            // dbsave

            await Clients.All.SendAsync("ReceiveMessage", name, message);
        }
    }
}
