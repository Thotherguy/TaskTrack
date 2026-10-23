using System.Security.Claims;
using TaskTrack.Models;
namespace TaskTrack.Data;
public static class Visibility
{
    public static IQueryable<Project> VisibleProjects(this ApplicationDbContext db, ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        bool admin = user.IsInRole("Admin");
        return db.Projects.Where(p => admin || p.OwnerId == id || p.Members.Any(m => m.UserId == id));
    }
    public static IQueryable<ProjectTask> VisibleTasks(this ApplicationDbContext db, ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        bool admin = user.IsInRole("Admin");
        bool manager = user.IsInRole("Manager");
        return db.ProjectTasks.Where(t => admin ||
            (manager && t.Project.OwnerId == id) ||
            (t.AssignedUserId == id && (t.Project.OwnerId == id || t.Project.Members.Any(m => m.UserId == id))));
    }
    public static IQueryable<ProjectActivity> VisibleActivity(this ApplicationDbContext db, ClaimsPrincipal user)
    {
        var projects = db.VisibleProjects(user).Select(p => p.Id);
        var tasks = db.VisibleTasks(user).Select(t => t.Id);
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        bool admin = user.IsInRole("Admin");
        bool manager = user.IsInRole("Manager");
        return db.ProjectActivities.Where(a => projects.Contains(a.ProjectId) &&
            (admin || (manager && a.Project.OwnerId == id) || a.ProjectTaskId == null || tasks.Contains(a.ProjectTaskId.Value)));
    }
}
