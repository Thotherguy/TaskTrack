
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Data;
using TaskTrack.Models;

namespace TaskTrack.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            string? userId = _userManager.GetUserId(User);

            var projects = await _context.Projects
                .Where(p => User.IsInRole("Admin") || p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await FindAccessibleProject(id.Value);

            if (project == null)
                return NotFound();

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(
            [Bind("Name,Description,StartDate,DueDate")] Project project)
        {
            if (project.DueDate.HasValue &&
                project.DueDate.Value.Date < project.StartDate.Date)
            {
                ModelState.AddModelError(
                    "DueDate",
                    "Due date cannot be before start date.");
            }

            // OwnerId is assigned on the server, not by the form.
            ModelState.Remove(nameof(Project.OwnerId));

            if (!ModelState.IsValid)
                return View(project);

            project.OwnerId = _userManager.GetUserId(User)!;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Project created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await FindOwnedProject(id.Value);

            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Name,Description,StartDate,DueDate")] Project input)
        {
            var project = await FindOwnedProject(id);

            if (project == null)
                return NotFound();

            if (input.DueDate.HasValue &&
                input.DueDate.Value.Date < input.StartDate.Date)
            {
                ModelState.AddModelError(
                    "DueDate",
                    "Due date cannot be before start date.");
            }

            ModelState.Remove(nameof(Project.OwnerId));

            if (!ModelState.IsValid)
            {
                input.Id = id;
                return View(input);
            }

            // Update only the fields users are allowed to change.
            project.Name = input.Name;
            project.Description = input.Description;
            project.StartDate = input.StartDate;
            project.DueDate = input.DueDate;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Project updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await FindOwnedProject(id.Value);

            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await FindOwnedProject(id);

            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Project deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Project?> FindAccessibleProject(int id)
        {
            var userId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");
            return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id &&
                (isAdmin || p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
        }

        // Restrict database lookups to the current user's projects.
        private async Task<Project?> FindOwnedProject(int id)
        {
            string? userId = _userManager.GetUserId(User);

            return await _context.Projects
                .FirstOrDefaultAsync(
                    p => p.Id == id && p.OwnerId == userId);
        }
    }
}
