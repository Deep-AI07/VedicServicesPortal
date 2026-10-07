using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VedicServicesPortal.Data;
using VedicServicesPortal.Models;

namespace VedicServicesPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER - GET
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                if (HttpContext.Session.GetString("UserRole") == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                return RedirectToAction("Index", "Service");
            }

            return View();
        }


        // =========================
        // REGISTER - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            // Clean inputs
            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                model.Username = model.Username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                model.Email = model.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
            }

            // 1. Validate Username uniqueness
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", "Username is required for Devotee login.");
            }
            else
            {
                var cleanUsername = model.Username.ToLower();
                var existingUsername = _context.Users
                    .FirstOrDefault(x => x.Username != null && x.Username.ToLower() == cleanUsername);

                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "This username is already taken. Please choose another username.");
                }
            }

            // 2. Validate Email uniqueness
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email address is required.");
            }
            else
            {
                var cleanEmail = model.Email.ToLower();
                var existingEmail = _context.Users
                    .FirstOrDefault(x => x.Email.ToLower() == cleanEmail);

                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                }
            }

            // 3. Validate Password Complexity (8 chars, 1 uppercase, 1 lowercase/char, 1 number, 1 special symbol)
            var (isPasswordValid, passwordError) = PasswordValidator.Validate(model.Password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("Password", passwordError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // All newly registered accounts are normal Users
            model.Role = "User";

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Sacred registration successful! You can now log in using your Username and Password.";

            return RedirectToAction("Login");
        }


        // =========================
        // LOGIN - GET
        // =========================

        [HttpGet]
        public IActionResult Login(string role = "user", string returnUrl = null)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                if (HttpContext.Session.GetString("UserRole") == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Service");
            }

            ViewBag.ActiveTab = role?.ToLower() == "admin" ? "admin" : "user";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        // =========================
        // LOGIN - POST
        // User logs in with Username & Password
        // Admin logs in with Email & Password
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string email, string identifier, string password, string loginRole = "User", string returnUrl = null)
        {
            // Default active tab for view rendering if error
            bool isAdminAttempt = (loginRole?.ToLower() == "admin") || 
                                  (!string.IsNullOrWhiteSpace(email)) || 
                                  (string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(identifier) && identifier.Contains("@"));

            ViewBag.ActiveTab = isAdminAttempt ? "admin" : "user";
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your password.";
                return View();
            }

            // ----------------------------------------------------
            // 1. ADMIN LOGIN: Authenticates via EMAIL and PASSWORD
            // ----------------------------------------------------
            if (isAdminAttempt)
            {
                var adminEmail = (email ?? identifier ?? "").Trim().ToLower();

                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    ViewBag.Error = "Please enter your admin email address.";
                    return View();
                }

                var adminUser = _context.Users
                    .FirstOrDefault(x => x.Role == "Admin" && x.Email.ToLower() == adminEmail);

                // Check credentials (supports updated "Admin@123" and legacy "123456" transition)
                bool isPassMatch = adminUser != null && (
                    adminUser.Password == password || 
                    (password == "123456" && adminUser.Password == "Admin@123") ||
                    (password == "Admin@123" && adminUser.Password == "123456")
                );

                if (adminUser == null || !isPassMatch)
                {
                    ViewBag.Error = "Invalid admin email or password.";
                    return View();
                }

                // Admin Login Succeeded
                HttpContext.Session.SetInt32("UserId", adminUser.UserId);
                HttpContext.Session.SetString("UserName", adminUser.Name);
                HttpContext.Session.SetString("UserRole", "Admin");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Dashboard", "Admin");
            }

            // ----------------------------------------------------
            // 2. DEVOTEE / USER LOGIN: Authenticated via USERNAME and PASSWORD
            // ----------------------------------------------------
            var devoteeUsername = (username ?? identifier ?? "").Trim();

            if (string.IsNullOrWhiteSpace(devoteeUsername))
            {
                ViewBag.Error = "Please enter your devotee username.";
                return View();
            }

            var devotee = _context.Users
                .FirstOrDefault(x => x.Role != "Admin" && (
                    (x.Username != null && x.Username.ToLower() == devoteeUsername.ToLower()) ||
                    (x.Username == null && x.Name.ToLower() == devoteeUsername.ToLower())
                ));

            if (devotee == null || devotee.Password != password)
            {
                ViewBag.Error = "Invalid devotee username or password.";
                return View();
            }

            // Devotee Login Succeeded
            HttpContext.Session.SetInt32("UserId", devotee.UserId);
            HttpContext.Session.SetString("UserName", devotee.Name);
            HttpContext.Session.SetString("UserRole", devotee.Role ?? "User");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Service");
        }


        // =========================
        // LOGOUT
        // =========================

        [HttpGet]
        public IActionResult Logout()
        {
            // Clear all login session data
            HttpContext.Session.Clear();

            // Clear session cookie from client
            Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToAction("Login");
        }
    }
}
