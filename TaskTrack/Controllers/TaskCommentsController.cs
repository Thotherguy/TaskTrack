using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Data;
using TaskTrack.Models;
namespace TaskTrack.Controllers;
[Authorize]
public class TaskCommentsController(ApplicationDbContext db) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int taskId, [Bind(Prefix = "Input")] CommentInput input)
    {
        var task = await db.VisibleTasks(User).Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return NotFound();
        input.Body = input.Body?.Trim() ?? "";
        if (!ModelState.IsValid || input.Body.Length == 0)
        {
            TempData["CommentError"] = "Enter a comment between 1 and 2,000 characters.";
            return RedirectToAction("Details", "ProjectTasks", new { id = taskId });
        }
        db.TaskComments.Add(new TaskComment { ProjectTaskId = task.Id, ProjectTask = task,
            AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!, Body = input.Body });
        await db.SaveChangesAsync();
        TempData["Success"] = "Comment added.";
        return RedirectToAction("Details", "ProjectTasks", new { id = taskId });
    }
}
