using JiraMini.Data;
using JiraMini.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraMini.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TaskDetailController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TaskDetailController(AppDbContext context) { _context = context; }

        private async Task<User> GetCurrentUserAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        private async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
            => await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

        // ===== TASK DETAIL =====
        // GET api/tasks/{id}/detail
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // Load task with relevant navigation properties
            var taskEntity = await _context.TaskItems
                .Include(t => t.Assignee)
                .Include(t => t.Status)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskEntity == null) return NotFound();

            var isMember = await IsMemberAsync(taskEntity.ProjectId, user.Id);
            if (!isMember) return Forbid();

            // Resolve creator and project owner usernames (Project.OwnerId is a Guid)
            var creator = await _context.Users.FindAsync(taskEntity.CreatorId);
            var projectOwner = taskEntity.Project != null ? await _context.Users.FindAsync(taskEntity.Project.OwnerId) : null;

            var task = new
            {
                taskEntity.Id,
                taskEntity.Title,
                taskEntity.Description,
                taskEntity.Priority,
                taskEntity.StatusId,
                StatusName = taskEntity.Status?.Name,
                taskEntity.AssigneeId,
                AssigneeName = taskEntity.Assignee?.Username,
                taskEntity.CreatorId,
                CreatorName = creator?.Username ?? projectOwner?.Username,
                taskEntity.DueDate,
                taskEntity.LabelColor,
                taskEntity.IsArchived,
                taskEntity.CreatedAt,
                taskEntity.UpdatedAt,
                taskEntity.ProjectId
            };

            var checklists = await _context.TaskChecklists
                .Where(c => c.TaskItemId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TaskChecklistDto { Id = c.Id, Content = c.Content, IsChecked = c.IsChecked, CreatedAt = c.CreatedAt })
                .ToListAsync();

            var comments = await _context.TaskComments
                .Where(c => c.TaskItemId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TaskCommentDto { Id = c.Id, Content = c.Content, AuthorId = c.AuthorId, AuthorName = c.Author.Username, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt })
                .ToListAsync();

            return Ok(new { task, checklists, comments });
        }

        // ===== CHECKLIST =====
        // POST api/tasks/{id}/checklists
        [HttpPost("{id}/checklists")]
        public async Task<IActionResult> AddChecklist(Guid id, [FromBody] CreateChecklistItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var isMember = await IsMemberAsync(task.ProjectId, user.Id);
            if (!isMember) return Forbid();

            var item = new TaskChecklist
            {
                Id = Guid.NewGuid(),
                TaskItemId = id,
                Content = dto.Content,
                IsChecked = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.TaskChecklists.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new TaskChecklistDto { Id = item.Id, Content = item.Content, IsChecked = item.IsChecked, CreatedAt = item.CreatedAt });
        }

        // PUT api/tasks/checklists/{checklistId}
        [HttpPut("checklists/{checklistId}")]
        public async Task<IActionResult> ToggleChecklist(Guid checklistId, [FromBody] UpdateChecklistItemDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var item = await _context.TaskChecklists
                .Include(c => c.TaskItem)
                .FirstOrDefaultAsync(c => c.Id == checklistId);
            if (item == null) return NotFound();

            var isMember = await IsMemberAsync(item.TaskItem.ProjectId, user.Id);
            if (!isMember) return Forbid();

            item.IsChecked = dto.IsChecked;
            await _context.SaveChangesAsync();
            return Ok(new TaskChecklistDto { Id = item.Id, Content = item.Content, IsChecked = item.IsChecked, CreatedAt = item.CreatedAt });
        }

        // DELETE api/tasks/checklists/{checklistId}
        [HttpDelete("checklists/{checklistId}")]
        public async Task<IActionResult> DeleteChecklist(Guid checklistId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var item = await _context.TaskChecklists
                .Include(c => c.TaskItem)
                .FirstOrDefaultAsync(c => c.Id == checklistId);
            if (item == null) return NotFound();

            var isMember = await IsMemberAsync(item.TaskItem.ProjectId, user.Id);
            if (!isMember) return Forbid();

            _context.TaskChecklists.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ===== COMMENTS =====
        // POST api/tasks/{id}/comments
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            var isMember = await IsMemberAsync(task.ProjectId, user.Id);
            if (!isMember) return Forbid();

            var comment = new TaskComment
            {
                Id = Guid.NewGuid(),
                TaskItemId = id,
                AuthorId = user.Id,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };
            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new TaskCommentDto { Id = comment.Id, Content = comment.Content, AuthorId = comment.AuthorId, AuthorName = user.Username, CreatedAt = comment.CreatedAt });
        }

        // PUT api/tasks/comments/{commentId}
        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var comment = await _context.TaskComments
                .Include(c => c.TaskItem)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return NotFound();
            if (comment.AuthorId != user.Id) return Forbid(); // chỉ author mới sửa được

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new TaskCommentDto { Id = comment.Id, Content = comment.Content, AuthorId = comment.AuthorId, AuthorName = user.Username, CreatedAt = comment.CreatedAt, UpdatedAt = comment.UpdatedAt });
        }

        // DELETE api/tasks/comments/{commentId}
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var comment = await _context.TaskComments
                .Include(c => c.TaskItem)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return NotFound();
            if (comment.AuthorId != user.Id) return Forbid();

            _context.TaskComments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}