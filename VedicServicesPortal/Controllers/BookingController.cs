using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VedicServicesPortal.Data;
using VedicServicesPortal.Models;
using VedicServicesPortal.ViewModels;

namespace VedicServicesPortal.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ============================================
        // CREATE - GET
        // ============================================

        [HttpGet]
        public IActionResult Create(int serviceId)
        {
            // Check Login
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { returnUrl = Url.Action("Create", "Booking", new { serviceId = serviceId }) });
            }

            if (serviceId <= 0)
            {
                var defaultSvc = _context.Services.FirstOrDefault(x => x.IsActive) ?? _context.Services.FirstOrDefault();
                if (defaultSvc != null)
                {
                    serviceId = defaultSvc.ServiceId;
                }
            }

            var service = _context.Services
                .FirstOrDefault(x =>
                    x.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound();
            }


            var pandits = _context.Pandits
                .Where(x => x.IsAvailable)
                .ToList();


            var model = new BookingViewModel
            {
                Service = service,

                ServiceId = service.ServiceId,

                Pandits = pandits,

                TimeSlots = _context.TimeSlots
                    .Where(x => !x.IsBooked)
                    .ToList()
            };


            return View(model);
        }


        // ============================================
        // GET TIME SLOTS - AJAX
        // ============================================

        [HttpGet]
        public IActionResult GetTimeSlots(int panditId, string date = null)
        {
            DateTime targetDate = DateTime.Today.AddDays(1);
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime parsedDate))
            {
                targetDate = parsedDate.Date;
            }

            // Check if slots already exist in database for this pandit and date
            var dbSlots = _context.TimeSlots
                .Where(x =>
                    x.PanditId == panditId &&
                    x.Date.Date == targetDate.Date &&
                    !x.IsBooked)
                .OrderBy(x => x.StartTime)
                .Select(x => new
                {
                    id = x.SlotId,
                    date = x.Date.ToString("dd-MM-yyyy"),
                    startTime = x.StartTime,
                    endTime = x.EndTime,
                    isPredefined = true
                })
                .ToList();

            if (dbSlots.Any())
            {
                return Json(dbSlots);
            }

            // If no specific pre-seeded slots exist for this future date,
            // provide standard auspicious Vedic Muhurat slots for user selection
            var defaultMuhurats = new[]
            {
                new { startTime = "06:00 AM", endTime = "08:00 AM", name = "Brahma Muhurat (Sunrise)" },
                new { startTime = "09:00 AM", endTime = "11:00 AM", name = "Auspicious Morning (Shubh)" },
                new { startTime = "11:45 AM", endTime = "01:45 PM", name = "Abhijit Muhurat (Vijaya)" },
                new { startTime = "02:30 PM", endTime = "04:30 PM", name = "Afternoon Muhurat (Amrit)" },
                new { startTime = "05:00 PM", endTime = "07:00 PM", name = "Sandhya Twilight (Labh)" },
                new { startTime = "07:00 PM", endTime = "09:00 PM", name = "Pradosh Kaal (Evening)" }
            };

            var dynamicSlots = defaultMuhurats.Select((m, index) => new
            {
                id = -(index + 1), // temporary identifier
                date = targetDate.ToString("dd-MM-yyyy"),
                startTime = m.startTime,
                endTime = m.endTime,
                name = m.name,
                isPredefined = false
            }).ToList();

            return Json(dynamicSlots);
        }


        // ============================================
        // CREATE - POST
        // ============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            BookingViewModel model)
        {
            // Check Login
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var service = _context.Services
                .FirstOrDefault(x =>
                    x.ServiceId == model.ServiceId);

            var pandit = _context.Pandits
                .FirstOrDefault(x =>
                    x.PanditId == model.PanditId &&
                    x.IsAvailable);

            if (service == null || pandit == null)
            {
                ModelState.AddModelError(
                    "",
                    "Please select a valid sacred service and Gurukul Pandit."
                );

                model.Service = service;
                model.Pandits = _context.Pandits.Where(x => x.IsAvailable).ToList();
                model.TimeSlots = _context.TimeSlots.Where(x => !x.IsBooked).ToList();
                return View(model);
            }

            // Ensure ceremony date is set (minimum today)
            DateTime ceremonyDate = model.BookingDate != default(DateTime) && model.BookingDate.Date >= DateTime.Today
                ? model.BookingDate.Date
                : DateTime.Today.AddDays(1);

            // Time slot resolution
            TimeSlot slot = null;

            if (model.SlotId > 0)
            {
                slot = _context.TimeSlots
                    .FirstOrDefault(x =>
                        x.SlotId == model.SlotId &&
                        x.PanditId == model.PanditId &&
                        !x.IsBooked);
            }

            if (slot == null)
            {
                // Dynamic or user-picked date and time slot
                string startTime = !string.IsNullOrWhiteSpace(model.StartTime) ? model.StartTime.Trim() : "09:00 AM";
                string endTime = !string.IsNullOrWhiteSpace(model.EndTime) ? model.EndTime.Trim() : "11:00 AM";

                // Check if existing slot matches this pandit, date, and startTime
                slot = _context.TimeSlots
                    .FirstOrDefault(x =>
                        x.PanditId == pandit.PanditId &&
                        x.Date.Date == ceremonyDate &&
                        x.StartTime == startTime);

                if (slot == null)
                {
                    slot = new TimeSlot
                    {
                        PanditId = pandit.PanditId,
                        Date = ceremonyDate,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsBooked = true
                    };
                    _context.TimeSlots.Add(slot);
                    _context.SaveChanges();
                }
                else if (slot.IsBooked)
                {
                    ModelState.AddModelError("", "This auspicious time slot has just been booked. Please select another slot or date.");
                    model.Service = service;
                    model.Pandits = _context.Pandits.Where(x => x.IsAvailable).ToList();
                    model.TimeSlots = _context.TimeSlots.Where(x => !x.IsBooked).ToList();
                    return View(model);
                }
            }

            // ============================================
            // PAYMENT VERIFICATION & CONSECRATION
            // Order is booked and invoice generated when payment is completed via Card/QR,
            // OR when devotee chooses Cash on Completion (in-person Dakshina after puja vidhi)
            // ============================================

            var selectedPayment = string.IsNullOrWhiteSpace(model.PaymentMethod) ? "UPI" : model.PaymentMethod;

            bool isCashOnCompletion = selectedPayment.Equals("Cash on Completion", StringComparison.OrdinalIgnoreCase)
                                      || selectedPayment.Equals("COD", StringComparison.OrdinalIgnoreCase)
                                      || selectedPayment.Equals("Cash", StringComparison.OrdinalIgnoreCase);

            if (!isCashOnCompletion && !model.IsPaymentCompleted)
            {
                ModelState.AddModelError("", "Dakshina payment must be completed via Card, UPI QR, or selected via Cash on Completion before the ceremony order can be booked.");
                model.Service = service;
                model.Pandits = _context.Pandits.Where(x => x.IsAvailable).ToList();
                model.TimeSlots = _context.TimeSlots.Where(x => !x.IsBooked).ToList();
                return View(model);
            }

            // Mark Slot Booked for the confirmed auspicious ceremony
            slot.IsBooked = true;

            // ============================================
            // CREATE BOOKING
            // ============================================

            var booking = new Booking
            {
                UserId = userId.Value,
                PanditId = pandit.PanditId,
                ServiceId = service.ServiceId,
                SlotId = slot.SlotId,
                BookingDate = slot.Date,
                Address = model.Address,
                TotalAmount = service.Price,
                PaymentMethod = isCashOnCompletion ? "Cash on Completion" : selectedPayment,
                Status = "Confirmed",
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return RedirectToAction(
                "Confirmation",
                new
                {
                    id = booking.BookingId
                });
        }


        // ============================================
        // CONFIRMATION
        // ============================================

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            var booking = _context.Bookings
                .FirstOrDefault(x =>
                    x.BookingId == id);


            if (booking == null)
            {
                return NotFound();
            }


            var service = _context.Services
                .FirstOrDefault(x =>
                    x.ServiceId ==
                    booking.ServiceId);


            var pandit = _context.Pandits
                .FirstOrDefault(x =>
                    x.PanditId ==
                    booking.PanditId);


            var slot = _context.TimeSlots
                .FirstOrDefault(x =>
                    x.SlotId ==
                    booking.SlotId);


            var model =
                new BookingConfirmationViewModel
                {
                    Booking = booking,

                    Service = service,

                    Pandit = pandit,

                    Slot = slot
                };


            return View(model);
        }


        // ============================================
        // MY BOOKINGS (WITH FILTERS: NAME, DATE, CATEGORY, VIDHI)
        // ============================================

        [HttpGet]
        public IActionResult MyBookings(string searchName = null, DateTime? searchDate = null, string category = null, string vidhi = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Pandit)
                .Include(b => b.TimeSlot)
                .Where(x => x.UserId == userId.Value);

            // 1. Search by Name (Pandit name or Address)
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var term = searchName.Trim().ToLower();
                query = query.Where(b => (b.Pandit != null && b.Pandit.Name.ToLower().Contains(term)) ||
                                         (b.Address != null && b.Address.ToLower().Contains(term)));
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
                .OrderByDescending(x => x.CreatedAt)
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


        // ============================================
        // INVOICE - GET (Print & Download PDF)
        // ============================================

        [HttpGet]
        public IActionResult Invoice(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Service)
                .Include(b => b.Pandit)
                .Include(b => b.TimeSlot)
                .FirstOrDefault(x => x.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Normal users can only see their own invoices, Admin can see all
            if (userRole != "Admin" && booking.UserId != userId.Value)
            {
                return Forbid();
            }

            // Sacred Dakshina invoice is only generated when payment has been completed and verified
            if (booking.Status != "Confirmed" && booking.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Sacred Invoice is only generated after Dakshina payment has been completed and verified.";
                return RedirectToAction("MyBookings");
            }

            return View(booking);
        }
    }
}
