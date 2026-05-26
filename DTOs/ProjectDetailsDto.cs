using System;
using System.Collections.Generic;

namespace JiraMini.Models
{
    public class ProjectMemberDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }

    public class ProjectDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ProjectKey { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProjectMemberDto> Members { get; set; }
    }
}
