namespace JiraMini.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? AvatarColor { get; set; } // THÊM: lưu gradient color key
        public DateTime CreatedAt { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; }
        public ICollection<TaskItem> AssignedTasks { get; set; }
    }
}
