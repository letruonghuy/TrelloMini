namespace TrelloMini.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ProjectKey { get; set; }
        public string Description { get; set; }

        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
    }


}
