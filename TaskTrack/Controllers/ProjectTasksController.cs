
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Data;
using TaskTrack.Models;

namespace TaskTrack.Controllers
{
    [Authorize]
    public class ProjectTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectTasksController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? CurrentUserId =>
            _userManager.GetUserId(User);

        private async Task<Project?> FindAccessibleProject(int projectId)
        {
            bool isAdmin = User.IsInRole("Admin");
            return await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId &&
                (isAdmin || p.OwnerId == CurrentUserId || p.Members.Any(m => m.UserId == CurrentUserId)));
        }

        private async Task<Project?> FindOwnedProject(int projectId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.Id == projectId &&
                    p.OwnerId == CurrentUserId);
        }

        private async Task<ProjectTask?> FindOwnedTask(int id)
        {
            return await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Project.OwnerId == CurrentUserId);
        }

        private void ValidateDueDate(TaskFormViewModel model, Project project)
        {
            if (model.DueDate.HasValue && model.DueDate.Value.Date < project.StartDate.Date)
                ModelState.AddModelError(nameof(model.DueDate), "Task due date cannot be before the project start date.");
            if (model.DueDate.HasValue && project.DueDate.HasValue && model.DueDate.Value.Date > project.DueDate.Value.Date)
                ModelState.AddModelError(nameof(model.DueDate), "Task due date cannot be after the project due date.");
        }

        private async Task LoadUsers(TaskFormViewModel model, int projectId)
        {
            model.Users = await _context.Users
                .Where(u => _context.Projects.Any(p => p.Id == projectId && p.OwnerId == u.Id) ||
                    _context.ProjectMembers.Any(m => m.ProjectId == projectId && m.UserId == u.Id))
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName ?? "Unknown"
                })
                .ToListAsync();
        }

        private async Task<bool> ValidateAssignee(
            TaskFormViewModel model, int projectId)
        {
            if (string.IsNullOrEmpty(model.AssignedUserId))
                return true;

            bool exists = await _context.Users
                .AnyAsync(u => u.Id == model.AssignedUserId && (_context.Projects.Any(p => p.Id == projectId && p.OwnerId == u.Id) ||
                    _context.ProjectMembers.Any(m => m.ProjectId == projectId && m.UserId == u.Id)));

            if (!exists)
            {
                ModelState.AddModelError(
                    nameof(model.AssignedUserId),
                    "The selected user must belong to this project.");
            }

            return exists;
        }

        // GET: ProjectTasks/Index?projectId=1
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await FindAccessibleProject(projectId);

            if (project == null)
                return NotFound();

            ViewBag.Project = project;

            var tasks = await _context.VisibleTasks(User)
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }

        public async Task<IActionResult> Details(int id, int commentPage = 1)
        {
            if (commentPage < 1 || commentPage > 10000) return BadRequest();
            var task = await _context.VisibleTasks(User).AsNoTracking().Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            var comments = await _context.TaskComments.AsNoTracking().Where(c => c.ProjectTaskId == id)
                .OrderByDescending(c => c.Id).Skip((commentPage - 1) * 20).Take(21).ToListAsync();
            var activity = await _context.ProjectActivities.AsNoTracking().Where(a => a.ProjectTaskId == id)
                .OrderByDescending(a => a.Id).Take(30).ToListAsync();
            var ids = comments.Select(c => c.AuthorId).Concat(activity.Where(a => a.ActorId != null).Select(a => a.ActorId!)).Distinct().ToList();
            var names = await _context.Users.AsNoTracking().Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "User");
            return View(new TaskDetailsViewModel { Task = task, Comments = comments.Take(20).ToList(),
                UserNames = names, CommentPage = commentPage, HasMoreComments = comments.Count > 20,
                Activity = activity });
        }

        // GET: ProjectTasks/Create?projectId=1
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(int projectId)
        {
            var project = await FindOwnedProject(projectId);

            if (project == null)
                return NotFound();

            ViewBag.Project = project;

            var model = new TaskFormViewModel();
            await LoadUsers(model, projectId);

            return View(model);
        }

        // POST: ProjectTasks/Create?projectId=1
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(
            int projectId, TaskFormViewModel model)
        {
            var project = await FindOwnedProject(projectId);

            if (project == null)
                return NotFound();

            ValidateDueDate(model, project);
            await ValidateAssignee(model, projectId);

            if (!ModelState.IsValid)
            {
                ViewBag.Project = project;
                await LoadUsers(model, projectId);
                return View(model);
            }

            var task = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                Priority = model.Priority,
                DueDate = model.DueDate,
                ProjectId = projectId,
                AssignedUserId = model.AssignedUserId
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task created successfully.";

            return RedirectToAction(nameof(Index),
                new { projectId });
        }

        // GET: ProjectTasks/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await FindOwnedTask(id);

            if (task == null)
                return NotFound();

            var model = new TaskFormViewModel
            {
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                AssignedUserId = task.AssignedUserId
            };

            ViewBag.TaskId = task.Id;
            ViewBag.ProjectId = task.ProjectId;

            await LoadUsers(model, task.ProjectId);
            return View(model);
        }

        // POST: ProjectTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(
            int id, TaskFormViewModel model)
        {
            var task = await FindOwnedTask(id);

            if (task == null)
                return NotFound();

            ValidateDueDate(model, task.Project);
            await ValidateAssignee(model, task.ProjectId);

            if (!ModelState.IsValid)
            {
                ViewBag.TaskId = task.Id;
                ViewBag.ProjectId = task.ProjectId;
                await LoadUsers(model, task.ProjectId);
                return View(model);
            }

            task.Title = model.Title;
            task.Description = model.Description;
            task.Status = model.Status;
            task.Priority = model.Priority;
            task.DueDate = model.DueDate;
            task.AssignedUserId = model.AssignedUserId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Task updated successfully.";

            return RedirectToAction(nameof(Index),
                new { projectId = task.ProjectId });
        }

        // GET: ProjectTasks/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await FindOwnedTask(id);

            if (task == null)
                return NotFound();

            return View(task);
        }

        // POST: ProjectTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await FindOwnedTask(id);

            if (task == null)
                return NotFound();

            int projectId = task.ProjectId;

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task deleted successfully.";

            return RedirectToAction(nameof(Index),
                new { projectId });
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id, string status)
        {
            if (status != "To Do" &&
                status != "In Progress" &&
                status != "Completed")
                return BadRequest();

            var task = await _context.ProjectTasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.AssignedUserId == CurrentUserId &&
                    t.Project.Members.Any(m =>
                        m.UserId == CurrentUserId));

            if (task == null)
                return NotFound();

            task.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index),
                new { projectId = task.ProjectId });
        }
    }
}
