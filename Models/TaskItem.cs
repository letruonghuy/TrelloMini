
namespace JiraMini.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }

        public int StatusId { get; set; }
        public TaskState Status { get; set; }

        public Guid CreatorId { get; set; }
        public Guid? AssigneeId { get; set; }
        public User Assignee { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public DateTime? DueDate { get; set; }        // Deadline
        public string? LabelColor { get; set; }        // Màu label: "red","blue","green","yellow","purple"
        public bool IsArchived { get; set; } = false;

    }
}




