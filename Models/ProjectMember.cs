namespace JiraMini.Models
{
    public class ProjectMember
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
    }

}
