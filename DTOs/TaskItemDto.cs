namespace TrelloMini.Models
{

    public class TaskItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public Guid? AssigneeId { get; set; }
        public string AssigneeName { get; set; }

        public DateTime? DueDate { get; set; }
        public string? LabelColor { get; set; }
        public bool IsArchived { get; set; }
        public string Description { get; set; } 
    }
}
