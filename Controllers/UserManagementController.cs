using Microsoft.AspNetCore.Mvc;
using UserApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // For ClaimTypes
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; // For SignOutAsync

namespace UserManagementApp.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.OrderBy(u => u.LastLoginTime).ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Block(int[] selectedUsers)
        {
            foreach (var userId in selectedUsers)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = false;
                }

            }
            await _context.SaveChangesAsync();

            // Check if the current user is in the selectedUsers list
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != null && selectedUsers.Contains(int.Parse(currentUserId)))
            {
                await HttpContext.SignOutAsync();
                TempData["ErrorMessage"] = "You have blocked yourself. Please contact an admin.";
                return RedirectToAction("Login", "Account");
            }

            TempData["SuccessMessage"] = "Selected users have been blocked successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(int[] selectedUsers)
        {
            foreach (var userId in selectedUsers)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Status = true;
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Selected users have been unblocked successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int[] selectedUsers)
        {
            foreach (var userId in selectedUsers)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Selected users have been deleted successfully.";

            // Redirect to login if the current user is deleted
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != null && selectedUsers.Contains(int.Parse(currentUserId)))
            {
                await HttpContext.SignOutAsync(); // Sign out the current user
                return RedirectToAction("Login");
            }

            return RedirectToAction("Index");
        }
    }
}