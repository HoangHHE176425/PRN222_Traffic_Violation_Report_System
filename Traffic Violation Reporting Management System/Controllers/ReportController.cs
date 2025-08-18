using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System.Models;
using Traffic_Violation_Reporting_Management_System.Service;
using Traffic_Violation_Reporting_Management_System.DTOs;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class ReportController : Controller
    {
        private readonly TrafficViolationDbContext _context;
        private readonly INotificationService _notifications;

        public ReportController(TrafficViolationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        [AuthorizeRole(1, 2)]
        public IActionResult ReportList(string search, int? status, string sortOrder)
        {
            var query = _context.Reports
                .Include(r => r.Reporter)
                .AsQueryable();

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.Location.Contains(search) ||
                    r.Reporter.FullName.Contains(search));
            }

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "violation_asc":
                    query = query.OrderBy(r => r.TimeOfViolation);
                    break;
                case "violation_desc":
                    query = query.OrderByDescending(r => r.TimeOfViolation);
                    break;
                case "created_asc":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "created_desc":
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
                case "name_asc":
                    query = query.OrderBy(r => r.Reporter.FullName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(r => r.Reporter.FullName);
                    break;
                default:
                    query = query.OrderByDescending(r => r.CreatedAt); // mặc định
                    break;
            }

            var result = query.ToList();
            return View("ReportList", result);
        }

        private int? GetCurrentUserIdFromSession()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        [AuthorizeRole(0)]
        public IActionResult ReportHistory(string search, int? status)
        {
            var userId = GetCurrentUserIdFromSession();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var query = _context.Reports
                .Include(r => r.Reporter)
                .Where(r => r.ReporterId == userId.Value)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Location.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status);
            }

            var history = query
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View("ReportHistory", history);
        }

        [HttpGet]
        [AuthorizeRole(0)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeRole(0)]
        public async Task<IActionResult> Create(Report report, IFormFile media)
        {
            var userId = GetCurrentUserIdFromSession();
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Clear and validate again
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(report.Location))
                ModelState.AddModelError(nameof(report.Location), "Địa điểm không được để trống.");

            if (!report.TimeOfViolation.HasValue)
                ModelState.AddModelError(nameof(report.TimeOfViolation), "Vui lòng chọn thời gian vi phạm.");
            else if (report.TimeOfViolation > DateTime.Now)
                ModelState.AddModelError(nameof(report.TimeOfViolation), "Thời gian vi phạm không được vượt quá thời điểm hiện tại.");

            if (string.IsNullOrWhiteSpace(report.Description))
                ModelState.AddModelError(nameof(report.Description), "Chú thích là bắt buộc.");

            if (media == null || media.Length == 0)
                ModelState.AddModelError("media", "Bạn cần tải lên ảnh hoặc video.");
            else if (media.Length > 100 * 1024 * 1024) // 100MB
                ModelState.AddModelError("media", "Tệp tải lên không được vượt quá 100MB.");

            if (!ModelState.IsValid)
                return View(report);

            try
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = Path.GetFileName(media.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await media.CopyToAsync(stream);
                }

                report.ReporterId = userId.Value;
                report.CreatedAt = DateTime.Now;
                report.MediaPath = "/uploads/" + fileName;
                report.MediaType = media.ContentType;

                // ✅ gán trạng thái mặc định
                report.Status = 0; // 0 = Pending/Chưa xử lý

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                // Gửi thông báo: báo cáo mới cho Officer
                var officers = await _context.Users
                    .Where(u => u.Role == 1 && u.IsActive == true)
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var officerId in officers)
                {
                    await _notifications.CreateAsync(
                        new CreateNotificationRequest(
                            officerId,
                            "report.created",
                            "Báo cáo mới",
                            $"Có báo cáo mới #{report.ReportId} tại {report.Location}.",
                            $"{{\"report_id\":{report.ReportId}}}"
                        )
                    );
                }

                TempData["SuccessMessage"] = "Báo cáo đã được gửi thành công.";
                return RedirectToAction("Detail", new { id = report.ReportId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                return View(report);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(1, 2)]
        public async Task<IActionResult> Reply(int id, string comment)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
            if (report == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Bạn phải nhập phản hồi trước khi gửi.";
                return RedirectToAction("Detail", new { id = report.ReportId });
            }

            // ✅ cập nhật trạng thái khi officer phản hồi
            report.Status = 1; // 1 = Đã phản hồi
            report.Comment = comment;

            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                new CreateNotificationRequest(
                    report.ReporterId,
                    "report.replied",
                    "Báo cáo của bạn đã được phản hồi",
                    $"Báo cáo #{report.ReportId} đã được phản hồi.",
                    $"{{\"report_id\":{report.ReportId}}}"
                )
            );

            TempData["SuccessMessage"] = "Phản hồi đã được gửi.";
            return RedirectToAction("Detail", new { id = report.ReportId });
        }

        [HttpGet]
        [AuthorizeRole(0, 1, 2)]
        public async Task<IActionResult> Detail(int id)
        {
            var report = await _context.Reports
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null) return NotFound();

            var role = HttpContext.Session.GetInt32("Role");
            ViewBag.IsPolice = (role == 1);
            return View(report);
        }
    }
}
