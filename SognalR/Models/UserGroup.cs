namespace SignalR.Models
{
    public class UserGroup
    {
        public Guid GroupId { get; set; }
        public string UserId { get; set; }
        public Group Group { get; set; }


    }
}
