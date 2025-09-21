namespace SignalR.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
