using System.ComponentModel.DataAnnotations;

namespace JiraMini.Models
{
    public class TaskCommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
    }

    public class UpdateCommentDto
    {
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
    }
}