using System.ComponentModel.DataAnnotations;

namespace SignalR.Models
{
    public class UserConnection
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string ConnectionId { get; set; }
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DisconnectedAt { get; set; }
        public bool IsConnected { get; set; } = true;
    }
}
