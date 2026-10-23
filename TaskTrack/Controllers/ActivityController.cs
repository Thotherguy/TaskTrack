using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Data;
using TaskTrack.Models;
namespace TaskTrack.Controllers;
[Authorize]
public class ActivityController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int projectId, int page = 1)
    {
        if (page < 1 || page > 10000) return BadRequest();
        var project = await db.VisibleProjects(User).AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return NotFound();
        var events = await db.VisibleActivity(User).AsNoTracking().Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.Id).Skip((page - 1) * 25).Take(26).ToListAsync();
        var actorIds = events.Where(a => a.ActorId != null).Select(a => a.ActorId!).Distinct().ToList();
        var names = await db.Users.AsNoTracking().Where(u => actorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "User");
        return View(new ProjectActivityViewModel { Project = project, Events = events.Take(25).ToList(),
            Page = page, HasMore = events.Count > 25, UserNames = names });
    }
}
