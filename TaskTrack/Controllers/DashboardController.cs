using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Data;
using TaskTrack.Models;
namespace TaskTrack.Controllers;
[Authorize]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tasks = db.VisibleTasks(User).AsNoTracking();
        var counts = await tasks.GroupBy(t => t.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
        var today = DateTime.Today;
        var model = new DashboardViewModel {
            ProjectCount = await db.VisibleProjects(User).CountAsync(),
            TaskCount = counts.Sum(c => c.Count),
            ToDoCount = counts.FirstOrDefault(c => c.Status == "To Do")?.Count ?? 0,
            InProgressCount = counts.FirstOrDefault(c => c.Status == "In Progress")?.Count ?? 0,
            CompletedCount = counts.FirstOrDefault(c => c.Status == "Completed")?.Count ?? 0,
            OverdueCount = await tasks.CountAsync(t => t.DueDate < today && t.Status != "Completed"),
            UpcomingTasks = await tasks.Include(t => t.Project).Where(t => t.Status != "Completed")
                .OrderBy(t => t.DueDate == null).ThenBy(t => t.DueDate).ThenBy(t => t.Id).Take(8).ToListAsync(),
            RecentActivity = await db.VisibleActivity(User).AsNoTracking().Include(a => a.Project)
                .OrderByDescending(a => a.Id).Take(10).ToListAsync()
        };
        var actorIds = model.RecentActivity.Where(a => a.ActorId != null).Select(a => a.ActorId!).Distinct().ToList();
        model.UserNames = await db.Users.AsNoTracking().Where(u => actorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "User");
        return View(model);
    }
}
