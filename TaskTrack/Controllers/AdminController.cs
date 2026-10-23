using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskTrack.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly TaskTrack.Data.ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser> users, TaskTrack.Data.ApplicationDbContext context)
        {
            _users = users;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _users.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(
            string userId, string role)
        {
            if (role != "Manager" && role != "Member")
                return BadRequest();

            var user = await _users.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (await _users.IsInRoleAsync(user, "Admin"))
                return BadRequest("Admin roles cannot be changed here.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var currentRoles = await _users.GetRolesAsync(user);

            var removableRoles = currentRoles
                .Where(r => r == "Manager" || r == "Member")
                .ToList();

            var removeResult = await _users.RemoveFromRolesAsync(
                user, removableRoles);

            if (!removeResult.Succeeded)
                return BadRequest("Could not remove existing roles.");

            var addResult = await _users.AddToRoleAsync(user, role);

            if (!addResult.Succeeded)
                return BadRequest("Could not assign the new role.");

            var stampResult = await _users.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded) return BadRequest("Could not refresh user permissions.");
            await transaction.CommitAsync();

            TempData["Success"] = "User role updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
