using System;

namespace TrelloMini.Models
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ProjectKey { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; } // THÊM

    }
}
