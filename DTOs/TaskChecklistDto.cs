using System.ComponentModel.DataAnnotations;

namespace JiraMini.Models
{
    public class TaskChecklistDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public bool IsChecked { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateChecklistItemDto
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
    }

    public class UpdateChecklistItemDto
    {
        [Required]
        public bool IsChecked { get; set; }
    }
}