namespace JiraMini.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? AvatarColor { get; set; } // THÊM

        public DateTime CreatedAt { get; set; }
    }
}
