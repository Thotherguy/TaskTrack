using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Data;
using TaskTrack.Models;
namespace TaskTrack.Controllers;
[Authorize(Roles = "Admin,Manager")]
public class ProjectMembersController(ApplicationDbContext db, UserManager<IdentityUser> users) : Controller
{
    private async Task<Project?> OwnedProject(int id) => await db.Projects
        .Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == users.GetUserId(User));
    private async Task<ProjectMembersViewModel> ViewModel(Project project, string email = "")
    {
        var ids = project.Members.Select(m => m.UserId).Append(project.OwnerId).ToList();
        return new() { Project = project, Email = email,
            Members = await db.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).OrderBy(u => u.Email).ToListAsync() };
    }
    public async Task<IActionResult> Index(int projectId)
    {
        var project = await OwnedProject(projectId);
        return project == null ? NotFound() : View(await ViewModel(project));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int projectId, [Bind("Email")] ProjectMembersViewModel input)
    {
        var project = await OwnedProject(projectId);
        if (project == null) return NotFound();
        ModelState.Remove(nameof(ProjectMembersViewModel.Project));
        if (!ModelState.IsValid) return View("Index", await ViewModel(project, input.Email));
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user == null)
        {
            ModelState.AddModelError(nameof(input.Email), "Unable to add a registered user with that email.");
            return View("Index", await ViewModel(project, input.Email));
        }
        if (user.Id == project.OwnerId || project.Members.Any(m => m.UserId == user.Id))
        {
            TempData["Success"] = "This user already belongs to the project.";
            return RedirectToAction(nameof(Index), new { projectId });
        }
        db.ProjectMembers.Add(new() { ProjectId = projectId, UserId = user.Id });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException error) when (error.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627)
        { TempData["Success"] = "This user already belongs to the project."; return RedirectToAction(nameof(Index), new { projectId }); }
        TempData["Success"] = "Project member added.";
        return RedirectToAction(nameof(Index), new { projectId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int projectId, string userId)
    {
        var project = await OwnedProject(projectId);
        if (project == null) return NotFound();
        if (userId == project.OwnerId) return BadRequest("The project owner cannot be removed.");
        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            var tasks = await db.ProjectTasks.Where(t => t.ProjectId == projectId && t.AssignedUserId == userId).ToListAsync();
            foreach (var task in tasks) task.AssignedUserId = null;
            db.ProjectMembers.Remove(member);
            await db.SaveChangesAsync();
        }
        TempData["Success"] = "Project member removed. Their task assignments were cleared.";
        return RedirectToAction(nameof(Index), new { projectId });
    }
}
