using System.ComponentModel.DataAnnotations;

namespace JiraMini.Models
{
    
    public class CreateTaskItemDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Priority { get; set; }

        [Required]
        public int StatusId { get; set; }

        public Guid? AssigneeId { get; set; }

        public DateTime? DueDate { get; set; }
        public string? LabelColor { get; set; }
    }
}
