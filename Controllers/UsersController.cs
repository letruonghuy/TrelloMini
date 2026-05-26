using JiraMini.Data;
using JiraMini.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraMini.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context) { _context = context; }

        private async Task<User?> GetCurrentUserAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // GET /api/users — danh sách tất cả user
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    AvatarColor = u.AvatarColor,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
            return Ok(users);
        }

        // GET /api/users/me — profile của user hiện tại kèm stats
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var projectCount = await _context.ProjectMembers
                .CountAsync(pm => pm.UserId == user.Id);

            var taskCount = await _context.TaskItems
                .CountAsync(t => t.AssigneeId == user.Id && !t.IsArchived);

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                avatarColor = user.AvatarColor,
                createdAt = user.CreatedAt,
                projectCount,
                taskCount
            });
        }

        // PUT /api/users/me — update username và avatarColor
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // Kiểm tra username trùng (trừ chính mình)
            var taken = await _context.Users
                .AnyAsync(u => u.Username == dto.Username && u.Id != user.Id);
            if (taken) return BadRequest("Tên hiển thị này đã được sử dụng.");

            user.Username = dto.Username;
            user.AvatarColor = dto.AvatarColor;
            await _context.SaveChangesAsync();

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarColor = user.AvatarColor,
                CreatedAt = user.CreatedAt
            });
        }
    }
}