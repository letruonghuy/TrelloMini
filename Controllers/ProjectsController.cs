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
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/projects - list projects current user is member of
        [HttpGet]
        public async Task<IActionResult> GetMyProjects()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();


            var projects = await _context.ProjectMembers
                .Where(pm => pm.UserId == user.Id)
                .Select(pm => pm.Project)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProjectKey = p.ProjectKey,
                    Description = p.Description,
                    OwnerId = p.OwnerId,
                    CreatedAt = p.CreatedAt,
                    MemberCount = _context.ProjectMembers.Count(m => m.ProjectId == p.Id) // THÊM

                })
                .ToListAsync();

            return Ok(projects);
        }

        // POST: api/projects - create project (current user becomes owner and member)
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ProjectKey = string.IsNullOrEmpty(dto.ProjectKey) ? dto.Name.Substring(0, Math.Min(3, dto.Name.Length)).ToUpper() : dto.ProjectKey,
                Description = dto.Description,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = user.Id, RoleId = 1 });
            await _context.SaveChangesAsync();

            var result = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                ProjectKey = project.ProjectKey,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt
            };

            return Ok(result);
        }

        // GET: api/projects/{id} - get project details with members
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();

            var isMember = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == id && pm.UserId == user.Id);
            if (!isMember) return Forbid();

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == id)
                .Join(_context.Users, pm => pm.UserId, u => u.Id, (pm, u) => new ProjectMemberDto
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleId = pm.RoleId
                })
                .ToListAsync();

            var dto = new ProjectDetailsDto
            {
                Id = project.Id,
                Name = project.Name,
                ProjectKey = project.ProjectKey,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreatedAt = project.CreatedAt,
                Members = members
            };

            return Ok(dto);
        }

        // POST: api/projects/{id}/members - add member by email (owner only)
        [HttpPost("{id}/members")]
        [Authorize(Policy = "ProjectOwner")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var member = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (member == null) return BadRequest("User with that email not found");

            var exists = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == id && pm.UserId == member.Id);
            if (exists) return BadRequest("User already a member");

            _context.ProjectMembers.Add(new ProjectMember { ProjectId = id, UserId = member.Id, RoleId = dto.RoleId });
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/projects/{id}/members/{userId} - remove member (owner only)
        [HttpDelete("{id}/members/{userId}")]
        [Authorize(Policy = "ProjectOwner")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
        {
            var pm = await _context.ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == id && x.UserId == userId);
            if (pm == null) return NotFound();

            _context.ProjectMembers.Remove(pm);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
