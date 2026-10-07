using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VedicServicesPortal.Data;
using VedicServicesPortal.Models;
using VedicServicesPortal.Services;

namespace VedicServicesPortal.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            ApplicationDbContext context,
            ISiteSettingService siteSettingService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _siteSettingService = siteSettingService;
            _environment = environment;
        }

        // ==========================================
        // ADMIN ACCESS CHECK
        // ==========================================

        private bool IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            return userId != null && role == "Admin";
        }


        // ==========================================
        // DASHBOARD
        // ==========================================

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalUsers = _context.Users.Count();

            ViewBag.TotalPandits = _context.Pandits.Count();

            ViewBag.TotalServices = _context.Services.Count();

            ViewBag.TotalTimeSlots = _context.TimeSlots.Count();

            ViewBag.TotalBookings = _context.Bookings.Count();

            ViewBag.PendingBookings =
                _context.Bookings.Count(x => x.Status == "Pending");

            ViewBag.ConfirmedBookings =
                _context.Bookings.Count(x => x.Status == "Confirmed");

            ViewBag.CompletedBookings =
                _context.Bookings.Count(x => x.Status == "Completed");

            ViewBag.CancelledBookings =
                _context.Bookings.Count(x => x.Status == "Cancelled");

            return View();
        }


        // ==========================================
        // USERS
        // ==========================================

        [HttpGet]
        public IActionResult Users()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _context.Users
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalAdmins = users.Count(u => u.Role == "Admin");
            ViewBag.TotalDevotees = users.Count(u => u.Role != "Admin");
            ViewBag.UserBookingCounts = _context.Bookings
                .Select(b => b.UserId)
                .ToList()
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(users);
        }


        // ==========================================
        // CREATE USER - GET
        // ==========================================

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new User { Role = "User" });
        }


        // ==========================================
        // CREATE USER - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Email))
            {
                model.Username = model.Email.Contains("@") ? model.Email.Substring(0, model.Email.IndexOf('@')) : model.Email;
            }

            if (!string.IsNullOrWhiteSpace(model.Username) && _context.Users.Any(u => u.Username.ToLower() == model.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "A user with this username already exists.");
            }

            if (_context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "A user with this email address already exists.");
            }

            var (isValidPass, passError) = PasswordValidator.Validate(model.Password);
            if (!isValidPass)
            {
                ModelState.AddModelError("Password", passError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                model.Role = "User";
            }

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["Success"] = $"User '{model.Name}' created successfully.";

            return RedirectToAction("Users");
        }


        // ==========================================
        // EDIT USER - GET
        // ==========================================

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // ==========================================
        // EDIT USER - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(int id, User model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.UserId)
            {
                return NotFound();
            }

            // Check if email belongs to another user
            if (_context.Users.Any(u => u.UserId != id && u.Email.ToLower() == model.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "This email address is already used by another user.");
            }

            // Check if username belongs to another user
            if (!string.IsNullOrWhiteSpace(model.Username) && _context.Users.Any(u => u.UserId != id && u.Username.ToLower() == model.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "This username is already used by another user.");
            }

            // Prevent self-demoting from Admin
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id && model.Role != "Admin")
            {
                ModelState.AddModelError("Role", "You cannot remove the Admin role from your own currently active session.");
            }

            // If password is left blank during edit, keep existing password
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.Remove("Password");
            }
            else
            {
                var (isValidPass, passError) = PasswordValidator.Validate(model.Password);
                if (!isValidPass)
                {
                    ModelState.AddModelError("Password", passError);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            user.Name = model.Name;
            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                user.Username = model.Username.Trim();
            }
            user.Email = model.Email.Trim();
            user.Role = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role;
            
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = model.Password;
            }

            _context.SaveChanges();

            TempData["Success"] = $"User '{user.Name}' updated successfully.";

            return RedirectToAction("Users");
        }


        // ==========================================
        // CHANGE USER PASSWORD (ADMIN MANAGE USER PASSWORDS)
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeUserPassword(int userId, string newPassword)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            var (isValid, errorMsg) = PasswordValidator.Validate(newPassword);
            if (!isValid)
            {
                TempData["Error"] = $"Password update failed for '{user.Name}': {errorMsg}";
                return RedirectToAction("Users");
            }

            user.Password = newPassword.Trim();
            _context.SaveChanges();

            TempData["Success"] = $"Password for user '{user.Name}' (ID: #{user.UserId}, Username: @{(user.Username ?? user.Name)}) has been successfully updated to '{newPassword}'.";
            return RedirectToAction("Users");
        }


        // ==========================================
        // DELETE USER - GET
        // ==========================================

        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.BookingCount = _context.Bookings.Count(b => b.UserId == id);
            ViewBag.IsSelf = (HttpContext.Session.GetInt32("UserId") == id);

            return View(user);
        }


        // ==========================================
        // DELETE USER - POST
        // ==========================================

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "Security Warning: You cannot delete your own logged-in administrator account.";
                return RedirectToAction("Users");
            }

            var bookingCount = _context.Bookings.Count(b => b.UserId == id);
            if (bookingCount > 0)
            {
                TempData["Error"] = $"Cannot delete user because they have {bookingCount} booking record(s). Please review their bookings first.";
                return RedirectToAction("Users");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = $"User '{user.Name}' has been removed successfully.";
            }

            return RedirectToAction("Users");
        }


        // ==========================================
        // PANDIT LIST
        // ==========================================

        [HttpGet]
        public IActionResult Pandits()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var pandits = _context.Pandits
                .OrderBy(x => x.Name)
                .ToList();

            return View(pandits);
        }


        // ==========================================
        // ADD PANDIT - GET
        // ==========================================

        [HttpGet]
        public IActionResult CreatePandit()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        // ==========================================
        // ADD PANDIT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePandit(Pandit model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Pandits.Add(model);
            _context.SaveChanges();

            TempData["Success"] =
                "Pandit added successfully.";

            return RedirectToAction("Pandits");
        }


        // ==========================================
        // EDIT PANDIT - GET
        // ==========================================

        [HttpGet]
        public IActionResult EditPandit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var pandit = _context.Pandits
                .FirstOrDefault(x => x.PanditId == id);

            if (pandit == null)
            {
                return NotFound();
            }

            return View(pandit);
        }


        // ==========================================
        // EDIT PANDIT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPandit(int id, Pandit model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.PanditId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var pandit = _context.Pandits
                .FirstOrDefault(x => x.PanditId == id);

            if (pandit == null)
            {
                return NotFound();
            }

            pandit.Name = model.Name;
            pandit.Mobile = model.Mobile;
            pandit.Email = model.Email;
            pandit.Experience = model.Experience;
            pandit.Specialization = model.Specialization;
            pandit.IsAvailable = model.IsAvailable;

            _context.SaveChanges();

            TempData["Success"] =
                "Pandit updated successfully.";

            return RedirectToAction("Pandits");
        }


        // ==========================================
        // PANDIT DETAILS
        // ==========================================

        [HttpGet]
        public IActionResult PanditDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var pandit = _context.Pandits
                .FirstOrDefault(x => x.PanditId == id);

            if (pandit == null)
            {
                return NotFound();
            }

            return View(pandit);
        }


        // ==========================================
        // DELETE PANDIT - GET
        // ==========================================

        [HttpGet]
        public IActionResult DeletePandit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var pandit = _context.Pandits
                .FirstOrDefault(x => x.PanditId == id);

            if (pandit == null)
            {
                return NotFound();
            }

            return View(pandit);
        }


        // ==========================================
        // DELETE PANDIT - POST
        // ==========================================

        [HttpPost]
        [ActionName("DeletePandit")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePanditConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var pandit = _context.Pandits
                .FirstOrDefault(x => x.PanditId == id);

            if (pandit == null)
            {
                return NotFound();
            }

            if (_context.Bookings.Any(b => b.PanditId == id))
            {
                TempData["Error"] = "Cannot delete Pandit because there are existing bookings associated with this Pandit.";
                return RedirectToAction("Pandits");
            }

            var slots = _context.TimeSlots.Where(t => t.PanditId == id).ToList();
            if (slots.Any())
            {
                _context.TimeSlots.RemoveRange(slots);
            }

            _context.Pandits.Remove(pandit);
            _context.SaveChanges();

            TempData["Success"] =
                "Pandit deleted successfully.";

            return RedirectToAction("Pandits");
        }


        // ==========================================
        // TOGGLE PANDIT AVAILABILITY
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TogglePanditAvailability(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var pandit = _context.Pandits
                .FirstOrDefault(x => x.PanditId == id);

            if (pandit == null)
            {
                return NotFound();
            }

            pandit.IsAvailable = !pandit.IsAvailable;

            _context.SaveChanges();

            return RedirectToAction("Pandits");
        }


        // ==========================================
        // TIME SLOT LIST
        // ==========================================

        [HttpGet]
        public IActionResult TimeSlots()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var slots = _context.TimeSlots
                .Include(x => x.Pandit)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToList();

            return View(slots);
        }


        // ==========================================
        // CREATE TIME SLOT - GET
        // ==========================================

        [HttpGet]
        public IActionResult CreateTimeSlot()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Pandits = _context.Pandits
                .Where(x => x.IsAvailable)
                .OrderBy(x => x.Name)
                .ToList();

            return View();
        }


        // ==========================================
        // CREATE TIME SLOT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTimeSlot(TimeSlot model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Pandits = _context.Pandits
                    .Where(x => x.IsAvailable)
                    .OrderBy(x => x.Name)
                    .ToList();

                return View(model);
            }

            _context.TimeSlots.Add(model);
            _context.SaveChanges();

            TempData["Success"] =
                "Time slot added successfully.";

            return RedirectToAction("TimeSlots");
        }


        // ==========================================
        // EDIT TIME SLOT - GET
        // ==========================================

        [HttpGet]
        public IActionResult EditTimeSlot(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var slot = _context.TimeSlots
                .FirstOrDefault(x => x.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            ViewBag.Pandits = _context.Pandits
                .OrderBy(x => x.Name)
                .ToList();

            return View(slot);
        }


        // ==========================================
        // EDIT TIME SLOT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTimeSlot(int id, TimeSlot model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.SlotId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Pandits = _context.Pandits
                    .OrderBy(x => x.Name)
                    .ToList();

                return View(model);
            }

            var slot = _context.TimeSlots
                .FirstOrDefault(x => x.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            slot.PanditId = model.PanditId;
            slot.Date = model.Date;
            slot.StartTime = model.StartTime;
            slot.EndTime = model.EndTime;
            slot.IsBooked = model.IsBooked;

            _context.SaveChanges();

            TempData["Success"] =
                "Time slot updated successfully.";

            return RedirectToAction("TimeSlots");
        }


        // ==========================================
        // DELETE TIME SLOT - GET
        // ==========================================

        [HttpGet]
        public IActionResult DeleteTimeSlot(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var slot = _context.TimeSlots
                .Include(x => x.Pandit)
                .FirstOrDefault(x => x.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            return View(slot);
        }


        // ==========================================
        // DELETE TIME SLOT - POST
        // ==========================================

        [HttpPost]
        [ActionName("DeleteTimeSlot")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTimeSlotConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var slot = _context.TimeSlots
                .FirstOrDefault(x => x.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            if (_context.Bookings.Any(b => b.SlotId == id))
            {
                TempData["Error"] = "Cannot delete time slot because it has associated customer bookings. Please cancel or remove the booking first.";
                return RedirectToAction("TimeSlots");
            }

            _context.TimeSlots.Remove(slot);
            _context.SaveChanges();

            TempData["Success"] =
                "Time slot deleted successfully.";

            return RedirectToAction("TimeSlots");
        }


        // ==========================================
        // TOGGLE TIME SLOT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTimeSlot(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var slot = _context.TimeSlots
                .FirstOrDefault(x => x.SlotId == id);

            if (slot == null)
            {
                return NotFound();
            }

            slot.IsBooked = !slot.IsBooked;

            _context.SaveChanges();

            return RedirectToAction("TimeSlots");
        }


        // ==========================================
        // BOOKINGS (WITH FILTERS: NAME, DATE, CATEGORY, VIDHI)
        // ==========================================

        [HttpGet]
        public IActionResult Bookings(string searchName = null, DateTime? searchDate = null, string category = null, string vidhi = null)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Pandit)
                .Include(b => b.Service)
                .Include(b => b.TimeSlot)
                .AsQueryable();

            // 1. Search by Name (Customer Name, Email, Username, or Pandit Name)
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var term = searchName.Trim().ToLower();
                query = query.Where(b => (b.User != null && b.User.Name.ToLower().Contains(term)) ||
                                         (b.User != null && b.User.Email.ToLower().Contains(term)) ||
                                         (b.User != null && b.User.Username != null && b.User.Username.ToLower().Contains(term)) ||
                                         (b.Pandit != null && b.Pandit.Name.ToLower().Contains(term)));
            }

            // 2. Search by Date
            if (searchDate.HasValue)
            {
                var d = searchDate.Value.Date;
                query = query.Where(b => b.BookingDate.Date == d || (b.TimeSlot != null && b.TimeSlot.Date.Date == d));
            }

            // 3. Search by Vidhi (Ceremony / Service Name)
            if (!string.IsNullOrWhiteSpace(vidhi))
            {
                var v = vidhi.Trim().ToLower();
                query = query.Where(b => b.Service != null && b.Service.ServiceName.ToLower().Contains(v));
            }

            var bookings = query
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            // 4. Search by Category (Puja, Havan, Jyotish, Griha)
            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                bookings = bookings.Where(b => b.Service != null &&
                    ServiceCategoryHelper.GetCategoryKey(b.Service.ServiceName).Equals(category.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.SearchName = searchName;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");
            ViewBag.Category = category ?? "all";
            ViewBag.Vidhi = vidhi;
            ViewBag.AllServices = _context.Services.Where(s => s.IsActive).OrderBy(s => s.ServiceName).ToList();

            return View(bookings);
        }


        // ==========================================
        // BOOKING DETAILS
        // ==========================================

        [HttpGet]
        public IActionResult BookingDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Pandit)
                .Include(b => b.Service)
                .Include(b => b.TimeSlot)
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }


        // ==========================================
        // INVOICE (ADMIN VIEW & PRINT INVOICE)
        // ==========================================

        [HttpGet]
        public IActionResult Invoice(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Invoice", "Booking", new { id = id });
        }


        // ==========================================
        // CONFIRM BOOKING
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmBooking(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = _context.Bookings
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Confirmed";

            _context.SaveChanges();

            TempData["Success"] =
                "Booking confirmed successfully.";

            return RedirectToAction("Bookings");
        }


        // ==========================================
        // COMPLETE BOOKING
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteBooking(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = _context.Bookings
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Completed";

            _context.SaveChanges();

            TempData["Success"] =
                "Booking marked as completed.";

            return RedirectToAction("Bookings");
        }


        // ==========================================
        // CANCEL BOOKING
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelBooking(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = _context.Bookings
                .Include(b => b.TimeSlot)
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Cancelled";

            if (booking.TimeSlot != null)
            {
                booking.TimeSlot.IsBooked = false;
            }

            _context.SaveChanges();

            TempData["Success"] =
                "Booking cancelled successfully.";

            return RedirectToAction("Bookings");
        }


        // ==========================================
        // SERVICES MANAGEMENT
        // ==========================================

        [HttpGet]
        public IActionResult Services()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var services = _context.Services
                .OrderBy(s => s.ServiceName)
                .ToList();

            ViewBag.TotalServices = services.Count;
            ViewBag.ActiveServices = services.Count(s => s.IsActive);
            ViewBag.InactiveServices = services.Count(s => !s.IsActive);
            ViewBag.BookingCounts = _context.Bookings
                .Select(b => b.ServiceId)
                .ToList()
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(services);
        }


        // ==========================================
        // CREATE SERVICE - GET
        // ==========================================

        [HttpGet]
        public IActionResult CreateService()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new Service { IsActive = true, Duration = 2, Price = 2100 });
        }


        // ==========================================
        // CREATE SERVICE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateService(Service model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Services.Add(model);
            _context.SaveChanges();

            TempData["Success"] = $"Service '{model.ServiceName}' created successfully.";

            return RedirectToAction("Services");
        }


        // ==========================================
        // EDIT SERVICE - GET
        // ==========================================

        [HttpGet]
        public IActionResult EditService(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var service = _context.Services.FirstOrDefault(s => s.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }


        // ==========================================
        // EDIT SERVICE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditService(int id, Service model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.ServiceId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var service = _context.Services.FirstOrDefault(s => s.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            service.ServiceName = model.ServiceName;
            service.Description = model.Description;
            service.Price = model.Price;
            service.Duration = model.Duration;
            service.IsActive = model.IsActive;

            _context.SaveChanges();

            TempData["Success"] = $"Service '{service.ServiceName}' updated successfully.";

            return RedirectToAction("Services");
        }


        // ==========================================
        // TOGGLE SERVICE STATUS
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleServiceStatus(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var service = _context.Services.FirstOrDefault(s => s.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            service.IsActive = !service.IsActive;
            _context.SaveChanges();

            TempData["Success"] = $"Service '{service.ServiceName}' is now {(service.IsActive ? "Active" : "Inactive")}.";

            return RedirectToAction("Services");
        }


        // ==========================================
        // DELETE SERVICE - GET
        // ==========================================

        [HttpGet]
        public IActionResult DeleteService(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var service = _context.Services.FirstOrDefault(s => s.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            ViewBag.BookingCount = _context.Bookings.Count(b => b.ServiceId == id);

            return View(service);
        }


        // ==========================================
        // DELETE SERVICE - POST
        // ==========================================

        [HttpPost, ActionName("DeleteService")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteServiceConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var bookingCount = _context.Bookings.Count(b => b.ServiceId == id);
            if (bookingCount > 0)
            {
                TempData["Error"] = $"Cannot delete service because it has {bookingCount} booking record(s). Deactivate it instead.";
                return RedirectToAction("Services");
            }

            var service = _context.Services.FirstOrDefault(s => s.ServiceId == id);
            if (service != null)
            {
                _context.Services.Remove(service);
                _context.SaveChanges();
                TempData["Success"] = $"Service '{service.ServiceName}' deleted successfully.";
            }

            return RedirectToAction("Services");
        }

        // ==========================================
        // PORTAL SETTINGS (BRAND, DESK, HERO, ETC)
        // ==========================================

        [HttpGet]
        public IActionResult Settings()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var settings = _siteSettingService.GetSettings();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SiteSetting model, IFormFile qrCodeImage, bool removeCustomQr = false)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = _context.SiteSettings.FirstOrDefault();
            if (existing == null)
            {
                model.LastUpdated = DateTime.UtcNow;
                _context.SiteSettings.Add(model);
                existing = model;
            }
            else
            {
                existing.BrandName = model.BrandName?.Trim();
                existing.BrandTagline = model.BrandTagline?.Trim();
                existing.BrandLogoOm = string.IsNullOrWhiteSpace(model.BrandLogoOm) ? "ॐ" : model.BrandLogoOm.Trim();
                existing.SiteDescription = model.SiteDescription?.Trim();
                existing.BrandStory = model.BrandStory?.Trim();

                existing.HeroHeadline = model.HeroHeadline?.Trim();
                existing.HeroSubheadline = model.HeroSubheadline?.Trim();
                existing.HeroDescription = model.HeroDescription?.Trim();
                existing.TrustStatsText = model.TrustStatsText?.Trim();

                existing.AcharyaDeskTitle = model.AcharyaDeskTitle?.Trim();
                existing.AcharyaDeskSubtitle = model.AcharyaDeskSubtitle?.Trim();
                existing.ContactPhone = model.ContactPhone?.Trim();
                existing.ContactEmail = model.ContactEmail?.Trim();
                existing.ContactAddress = model.ContactAddress?.Trim();
                existing.HelplineHours = model.HelplineHours?.Trim();
                existing.EmergencyWhatsApp = model.EmergencyWhatsApp?.Trim();
                existing.AcharyaInquiryResponseTime = model.AcharyaInquiryResponseTime?.Trim();

                existing.GuaranteeBadgeText = model.GuaranteeBadgeText?.Trim();
                existing.FooterCopyrightText = model.FooterCopyrightText?.Trim();
                existing.UpiId = model.UpiId?.Trim();
                existing.LastUpdated = DateTime.UtcNow;

                _context.SiteSettings.Update(existing);
            }

            // Handle Custom QR Image Removal
            if (removeCustomQr)
            {
                if (!string.IsNullOrEmpty(existing.QrCodeImagePath))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, existing.QrCodeImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { }
                    }
                    existing.QrCodeImagePath = null;
                }
            }
            // Handle Custom QR Image Upload
            else if (qrCodeImage != null && qrCodeImage.Length > 0)
            {
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp", ".svg" };
                var ext = Path.GetExtension(qrCodeImage.FileName).ToLowerInvariant();
                if (allowedExts.Contains(ext))
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Delete existing QR image file if present
                    if (!string.IsNullOrEmpty(existing.QrCodeImagePath))
                    {
                        var oldPath = Path.Combine(_environment.WebRootPath, existing.QrCodeImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            try { System.IO.File.Delete(oldPath); } catch { }
                        }
                    }

                    var fileName = $"upi_qr_{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await qrCodeImage.CopyToAsync(stream);
                    }

                    existing.QrCodeImagePath = $"/uploads/{fileName}";
                }
            }

            await _context.SaveChangesAsync();
            _siteSettingService.ClearCache();

            TempData["SuccessMessage"] = "✨ Portal brand identity, descriptions, Contact Acharya Desk, and QR code settings saved successfully!";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetSettings()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var existing = _context.SiteSettings.FirstOrDefault();
            if (existing != null)
            {
                // Delete custom QR image if present
                if (!string.IsNullOrEmpty(existing.QrCodeImagePath))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, existing.QrCodeImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { }
                    }
                }

                existing.BrandName = "VedicServicesPortal";
                existing.BrandTagline = "Faith • Tradition • Blessed Living";
                existing.BrandLogoOm = "ॐ";
                existing.SiteDescription = "Authentic Vedic rituals, sacred pujas, and verified Gurukul Pandits booking portal. Faith • Tradition • Blessed Living.";
                existing.BrandStory = "Bridging timeless Vedic spirituality and contemporary technology. Consecrated with devotion, our mission is to ensure every family experiences pure Shastric rituals, sacred mantras, and learned Gurukul Acharyas.";
                existing.HeroHeadline = "Bring Divine Blessings";
                existing.HeroSubheadline = "Into Your Home";
                existing.HeroDescription = "Connect with experienced, verified Pandits for authentic Vedic rituals, sacred pujas, and sanctified ceremonies — conveniently booked online with absolute Shastra vidhi.";
                existing.TrustStatsText = "4.9/5 from 12,500+ sacred ceremonies performed across India & Diaspora";
                existing.AcharyaDeskTitle = "Contact Acharya Desk";
                existing.AcharyaDeskSubtitle = "Dedicated Acharya desk for astrological guidance & rituals query.";
                existing.ContactPhone = "+91 8780495951";
                existing.ContactEmail = "vivekmaghudiya6@gmail.com";
                existing.ContactAddress = "Ramnagar, Jam-khambhalya";
                existing.HelplineHours = "6:00 AM - 9:30 PM (All 7 Days)";
                existing.EmergencyWhatsApp = "+91 8780495951";
                existing.AcharyaInquiryResponseTime = "Our senior Purohit will call your contact number within 4 business hours.";
                existing.GuaranteeBadgeText = "SHASTRA CERTIFIED • AUTHENTICITY GUARANTEE";
                existing.FooterCopyrightText = "Sanctified with devotion & authentic Vedic tradition.";
                existing.UpiId = "vedicservices@upi";
                existing.QrCodeImagePath = null;
                existing.LastUpdated = DateTime.UtcNow;

                _context.SiteSettings.Update(existing);
                _context.SaveChanges();
            }

            _siteSettingService.ClearCache();
            TempData["SuccessMessage"] = "↺ Portal settings restored to original Sanatana defaults.";
            return RedirectToAction("Settings");
        }
    }
}
