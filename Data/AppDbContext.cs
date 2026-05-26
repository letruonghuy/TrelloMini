using Microsoft.EntityFrameworkCore;
using TrelloMini.Models;

namespace TrelloMini.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskState> TaskStates { get; set; }
        public DbSet<TaskChecklist> TaskChecklists { get; set; }  // THÊM
        public DbSet<TaskComment> TaskComments { get; set; }       // THÊM

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.UserId });

            // Checklist → TaskItem
            modelBuilder.Entity<TaskChecklist>()
                .HasOne(c => c.TaskItem)
                .WithMany()
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → TaskItem
            modelBuilder.Entity<TaskComment>()
                .HasOne(c => c.TaskItem)
                .WithMany()
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → Author (User) - NO cascade để tránh multiple cascade paths
            modelBuilder.Entity<TaskComment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // ===== SEED DATA (giữ nguyên) =====
            var adminId = Guid.Parse("0c5939f5-25a3-49fc-1731-08de5c89da4f");
            var projectId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@jiramini.com",
                PasswordHash = "123456",
                CreatedAt = new DateTime(2026, 1, 26, 11, 50, 10, 15, DateTimeKind.Utc)
            });

            modelBuilder.Entity<TaskState>().HasData(
                new TaskState { Id = 1, Name = "To Do", OrderIndex = 1 },
                new TaskState { Id = 2, Name = "In Progress", OrderIndex = 2 },
                new TaskState { Id = 3, Name = "Done", OrderIndex = 3 }
            );

            modelBuilder.Entity<Project>().HasData(new Project
            {
                Id = projectId,
                Name = "Jira Mini",
                ProjectKey = "JM",
                Description = "Jira Mini for DACN",
                OwnerId = adminId,
                CreatedAt = new DateTime(2026, 1, 26, 11, 50, 10, 15, DateTimeKind.Utc)
            });

            modelBuilder.Entity<ProjectMember>().HasData(new ProjectMember
            {
                ProjectId = projectId,
                UserId = adminId,
                RoleId = 1
            });
        }
    }
}