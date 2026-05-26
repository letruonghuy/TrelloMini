using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using TrelloMini.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System;

namespace TrelloMini.Authorization
{
    public class ProjectOwnerHandler : AuthorizationHandler<ProjectOwnerRequirement>
    {
        private readonly AppDbContext _db;

        public ProjectOwnerHandler(AppDbContext db)
        {
            _db = db;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectOwnerRequirement requirement)
        {
            // Try to get HttpContext from resource
            HttpContext httpContext = null;

            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                httpContext = mvcContext.HttpContext;
            }
            else if (context.Resource is HttpContext directHttp)
            {
                httpContext = directHttp;
            }

            if (httpContext == null)
                return;

            // route param name is 'id'
            if (!httpContext.Request.RouteValues.TryGetValue("id", out var idObj))
                return;

            if (!Guid.TryParse(idObj?.ToString(), out var projectId))
                return;

            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return;

            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                return;

            if (project.OwnerId == user.Id)
            {
                context.Succeed(requirement);
            }
        }
    }
}
