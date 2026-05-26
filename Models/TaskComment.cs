namespace JiraMini.Models
{
    public class TaskComment
    {
        public Guid Id { get; set; }
        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }
        public Guid AuthorId { get; set; }
        public User Author { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}