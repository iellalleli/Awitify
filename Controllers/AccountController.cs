using Microsoft.AspNetCore.Mvc;
using KaraokeApp.Data;
using KaraokeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace KaraokeApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = password, 
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.PasswordHash == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("HomePage", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}