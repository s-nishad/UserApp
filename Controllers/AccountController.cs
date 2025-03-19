using Microsoft.AspNetCore.Mvc;
using UserApp.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UserApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
                };

                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User Created Successfully!";
                    return RedirectToAction("Login");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    Console.Write(ex);
                }
            }
            return View(model);
        }

        // login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                // Check if the user exists and the password is correct
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Check if the user is blocked
                    if (user.Status == false)
                    {
                        ModelState.AddModelError("", "Your account is blocked.");
                        return View(model);
                    }

                    // Update the last login time
                    user.LastLoginTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Sign in the user (set authentication cookie)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.Name)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Ensure the user is authenticated
                    if (User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        // Log the successful authentication
                        Console.WriteLine("User authenticated successfully.");

                        // Redirect to the User Management page
                        return RedirectToAction("Index", "UserManagement");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Authentication failed.");
                        Console.WriteLine("Authentication failed.");
                    }
                }

                // If login fails, show an error message
                ModelState.AddModelError("", "Invalid email or password.");
                Console.WriteLine("Invalid email or password.");
            }

            // If validation fails, show the login form again
            return View(model);
        }
    }
}