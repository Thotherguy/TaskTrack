using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Models;

namespace TaskTrack.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? http = null)
            : base(options)
        {
            _http = http;
        }

        private readonly IHttpContextAccessor? _http;
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<ProjectActivity> ProjectActivities { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<TaskComment>().HasOne(c => c.ProjectTask).WithMany()
                .HasForeignKey(c => c.ProjectTaskId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<TaskComment>().HasIndex(c => new { c.ProjectTaskId, c.CreatedAtUtc });
            builder.Entity<ProjectActivity>().HasOne(a => a.Project).WithMany()
                .HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ProjectActivity>().HasIndex(a => new { a.ProjectId, a.CreatedAtUtc });

            builder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();

            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Write history in the same transaction as the change. Clients cannot submit audit fields.
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();
            var deletedProjects = ChangeTracker.Entries<Project>()
                .Where(e => e.State == EntityState.Deleted).Select(e => e.Entity.Id).ToHashSet();
            var pending = new List<Func<ProjectActivity>>();
            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
                var state = entry.State;
                if (entry.Entity is Project project && state != EntityState.Deleted)
                {
                    var summary = state == EntityState.Added ? "Project created" : "Project updated";
                    pending.Add(() => new() { ProjectId = project.Id, Summary = summary });
                }
                if (entry.Entity is ProjectTask task && !deletedProjects.Contains(task.ProjectId))
                {
                    string summary = state switch {
                        EntityState.Added => $"Task created: {task.Title}",
                        EntityState.Deleted => $"Task deleted: {task.Title}",
                        _ => $"Task updated: {task.Title}"
                    };
                    if (state == EntityState.Modified && entry.Property(nameof(ProjectTask.Status)).IsModified)
                        summary = $"Status changed from {entry.Property(nameof(ProjectTask.Status)).OriginalValue} to {task.Status}: {task.Title}";
                    var captured = summary;
                    pending.Add(() => new() { ProjectId = task.ProjectId, ProjectTaskId = task.Id, Summary = captured });
                }
                if (entry.Entity is ProjectMember member && !deletedProjects.Contains(member.ProjectId))
                {
                    var summary = state == EntityState.Deleted ? "Project member removed" : "Project member added";
                    pending.Add(() => new() { ProjectId = member.ProjectId, Summary = summary });
                }
                if (entry.Entity is TaskComment comment && state == EntityState.Added)
                    pending.Add(() => new() { ProjectId = comment.ProjectTask.ProjectId,
                        ProjectTaskId = comment.ProjectTaskId, Summary = "Comment added" });
            }
            if (pending.Count == 0) return await base.SaveChangesAsync(cancellationToken);
            var transaction = Database.CurrentTransaction == null
                ? await Database.BeginTransactionAsync(cancellationToken) : null;
            try
            {
                int result = await base.SaveChangesAsync(cancellationToken);
                string? actor = _http?.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                foreach (var factory in pending)
                {
                    var activity = factory(); activity.ActorId = actor;
                    ProjectActivities.Add(activity);
                }
                await base.SaveChangesAsync(cancellationToken);
                if (transaction != null) await transaction.CommitAsync(cancellationToken);
                return result;
            }
            finally { if (transaction != null) await transaction.DisposeAsync(); }
        }

    }
}
