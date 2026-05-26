using JiraMini.Data;
using JiraMini.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace JiraMini.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskItemsController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var user = HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return null;

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        private async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        // 1️⃣ Lấy tất cả task theo project (Kanban board)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId && !t.IsArchived) // lọc bỏ archived
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.Username,
                    DueDate = t.DueDate,
                    LabelColor = t.LabelColor,
                    IsArchived = t.IsArchived
                })
                .ToListAsync();

            return Ok(tasks);
        }


        // 2️⃣ Tạo task mới
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creator = await GetCurrentUserAsync();
            if (creator == null)
                return Unauthorized();

            // validate project exists
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null)
                return BadRequest("Project not found");

            // check membership
            var isMember = await IsMemberAsync(dto.ProjectId, creator.Id);
            if (!isMember)
                return Forbid();

            // validate status exists
            var statusExists = await _context.TaskStates.AnyAsync(s => s.Id == dto.StatusId);
            if (!statusExists)
                return BadRequest("Invalid statusId");

            // validate assignee if provided
            if (dto.AssigneeId.HasValue)
            {
                var assigneeExists = await _context.Users.AnyAsync(u => u.Id == dto.AssigneeId.Value);
                if (!assigneeExists)
                    return BadRequest("Assignee not found");
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                StatusId = dto.StatusId,
                CreatorId = creator.Id,
                AssigneeId = dto.AssigneeId,
                DueDate = dto.DueDate,       // THÊM
                LabelColor = dto.LabelColor, // THÊM
                IsArchived = false,          // THÊM
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            // return a DTO (avoid returning EF entity with navigation properties that cause cycles)
            var created = await _context.TaskItems
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Priority = t.Priority,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.Username
                })
                .FirstOrDefaultAsync();

            return Ok(created);
        }

        // ĐỔI TÊN endpoint thành archive, giữ nguyên route DELETE để frontend không cần sửa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var isMember = await IsMemberAsync(task.ProjectId, user.Id);
            if (!isMember) return Forbid();

            // ARCHIVE thay vì xóa thật
            task.IsArchived = true;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Update task fields: title, description, priority, assignee
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var isMember = await IsMemberAsync(task.ProjectId, user.Id);
            if (!isMember) return Forbid();

            // validate assignee if provided
            if (dto.AssigneeId.HasValue)
            {
                var assigneeExists = await _context.Users.AnyAsync(u => u.Id == dto.AssigneeId.Value);
                if (!assigneeExists) return BadRequest("Assignee not found");
                task.AssigneeId = dto.AssigneeId.Value;
            }
            else
            {
                task.AssigneeId = null;
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate;         // THÊM
            task.LabelColor = dto.LabelColor;   // THÊM
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.TaskItems
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Priority = t.Priority,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.Username
                })
                .FirstOrDefaultAsync();

            return Ok(updated);
        }

        // 3️⃣ Đổi trạng thái task (drag & drop)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] int statusId)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var isMember = await IsMemberAsync(task.ProjectId, user.Id);
            if (!isMember) return Forbid();

            var statusExists = await _context.TaskStates.AnyAsync(s => s.Id == statusId);
            if (!statusExists) return BadRequest("Invalid statusId");

            task.StatusId = statusId;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.TaskItems
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Priority = t.Priority,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.Username
                })
                .FirstOrDefaultAsync();

            return Ok(updated);
        }

        // 4️⃣ Assign task cho user (nullable to unassign)
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(Guid id, [FromBody] Guid? userId)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var actor = await GetCurrentUserAsync();
            if (actor == null) return Unauthorized();

            var isMember = await IsMemberAsync(task.ProjectId, actor.Id);
            if (!isMember) return Forbid();

            if (userId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value);
                if (!userExists) return BadRequest("User not found");

                task.AssigneeId = userId.Value;
            }
            else
            {
                task.AssigneeId = null; // unassign
            }

            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.TaskItems
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Priority = t.Priority,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.Username
                })
                .FirstOrDefaultAsync();

            return Ok(updated);
        }

        // GET archived tasks của project
        [HttpGet("project/{projectId}/archived")]
        public async Task<IActionResult> GetArchivedByProject(Guid projectId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId && t.IsArchived)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.Username,
                    DueDate = t.DueDate,
                    LabelColor = t.LabelColor,
                    IsArchived = t.IsArchived
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // Permanently delete archived tasks of a project
        [HttpDelete("project/{projectId}/archived")]
        public async Task<IActionResult> DeleteArchivedByProject(Guid projectId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var isMember = await IsMemberAsync(projectId, user.Id);
            if (!isMember) return Forbid();

            var archivedTasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId && t.IsArchived)
                .ToListAsync();

            if (archivedTasks.Count == 0)
            {
                return Ok(new { deleted = 0 });
            }

            _context.TaskItems.RemoveRange(archivedTasks);
            await _context.SaveChangesAsync();

            return Ok(new { deleted = archivedTasks.Count });
        }

        // Restore task từ archive
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreTask(Guid id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var isMember = await IsMemberAsync(task.ProjectId, user.Id);
            if (!isMember) return Forbid();

            task.IsArchived = false;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
