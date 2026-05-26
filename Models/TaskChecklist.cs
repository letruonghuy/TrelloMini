namespace JiraMini.Models
{
    public class TaskChecklist
    {
        public Guid Id { get; set; }
        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }
        public string Content { get; set; }
        public bool IsChecked { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}