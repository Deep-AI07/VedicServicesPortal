using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VedicServicesPortal.Data;

namespace VedicServicesPortal.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Where(x => x.IsActive)
                .ToListAsync();

            return View(services);
        }

        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(x => x.ServiceId == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpGet]
        public async Task<IActionResult> Suggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                var popular = await _context.Services
                    .Where(x => x.IsActive)
                    .Take(6)
                    .Select(s => new {
                        serviceId = s.ServiceId,
                        serviceName = s.ServiceName,
                        price = s.Price,
                        duration = s.Duration,
                        description = s.Description
                    })
                    .ToListAsync();
                return Json(popular);
            }

            var cleanQ = q.ToLower().Trim();
            var matches = await _context.Services
                .Where(x => x.IsActive && (
                    x.ServiceName.ToLower().Contains(cleanQ) || 
                    x.Description.ToLower().Contains(cleanQ)))
                .Select(s => new {
                    serviceId = s.ServiceId,
                    serviceName = s.ServiceName,
                    price = s.Price,
                    duration = s.Duration,
                    description = s.Description
                })
                .ToListAsync();

            return Json(matches);
        }
    }
}