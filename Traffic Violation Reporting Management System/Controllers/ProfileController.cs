using Microsoft.AspNetCore.Mvc;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class ProfileController : Controller
    {
        private readonly TrafficViolationDbContext _context;

        public ProfileController(TrafficViolationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();
            var vehicles = _context.Vehicles
                           .Where(v => v.OwnerCccd == user.Cccd)
                           .ToList();

            // truyền qua ViewBag
            ViewBag.Vehicles = vehicles;

            return View(user);
        }

    }
}
