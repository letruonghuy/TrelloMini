using System.ComponentModel.DataAnnotations;

namespace JiraMini.Models
{
    public class AddMemberDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // RoleId optional, default to 2 (member)
        public int RoleId { get; set; } = 2;
    }
}
