using System.ComponentModel.DataAnnotations;

namespace TrelloMini.Models
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Username { get; set; }

        [StringLength(500)]
        public string? AvatarColor { get; set; }
    }
}