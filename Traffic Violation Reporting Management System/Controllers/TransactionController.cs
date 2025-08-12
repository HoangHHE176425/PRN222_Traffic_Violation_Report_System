using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TrafficViolationDbContext _context;

        public TransactionController(TrafficViolationDbContext context)
        {
            _context = context;
        }

        // --- Lấy userId từ session ---
        private int? GetCurrentUserIdFromSession()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // --- Danh sách tất cả giao dịch ---
        [AuthorizeRole(1,2)]

        public IActionResult TransactionList(string search, string sortOrder)
        {
            var query = _context.Transactions
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.User.FullName.Contains(search));
            }

            switch (sortOrder)
            {
                case "amount_asc":
                    query = query.OrderBy(t => t.Amount);
                    break;
                case "amount_desc":
                    query = query.OrderByDescending(t => t.Amount);
                    break;
                case "created_asc":
                    query = query.OrderBy(t => t.CreatedAt);
                    break;
                case "created_desc":
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
            }

            var result = query.ToList();
            return View("TransactionList", result);
        }

        // --- Xem chi tiết 1 giao dịch ---
        [HttpGet]
        [AuthorizeRole(0,1,2)]

        public IActionResult Detail(int id)
        {
            var transaction = _context.Transactions
                .Include(t => t.User)
                .FirstOrDefault(t => t.TransactionId == id);

            if (transaction == null)
                return NotFound();

            return View("Detail", transaction);
        }

        // --- Lịch sử giao dịch của người dùng ---
        [AuthorizeRole(0)]

        public IActionResult TransactionHistory()
        {
            var userId = GetCurrentUserIdFromSession();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var history = _context.Transactions
                .Where(t => t.UserId == userId.Value)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return View("TransactionHistory", history);
        }
    }
}
