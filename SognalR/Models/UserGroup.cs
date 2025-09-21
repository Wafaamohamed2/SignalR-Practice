namespace SignalR.Models
{
    public class UserGroup
    {
        public int GroupId { get; set; }
        public string UserId { get; set; }
        public Group Group { get; set; }


    }
}
