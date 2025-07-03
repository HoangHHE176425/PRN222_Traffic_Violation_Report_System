using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class ReportController : Controller
    {
        private readonly TrafficViolationDbContext _context;

        public ReportController(TrafficViolationDbContext context)
        {
            _context = context;
        }

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

            // Sắp xếp theo lựa chọn
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

        private int? GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // Tạm dùng UserId = 31 nếu chưa đăng nhập (chỉ dùng khi test)
                // Khi có hệ thống login thì bỏ dòng dưới
                userId = 31;
                HttpContext.Session.SetInt32("UserId", 31);
            }

            return userId;
        }

        public IActionResult ReportHistory(string search, int? status)
        {
            var userId = GetCurrentUserId();
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
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Report report, IFormFile media)
        {
            var userId = GetCurrentUserId();

            // ✅ Tự kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(report.Location))
                ModelState.AddModelError("Location", "Vui lòng nhập địa điểm.");

            if (!report.TimeOfViolation.HasValue)
                ModelState.AddModelError("TimeOfViolation", "Vui lòng chọn thời gian vi phạm.");

            if (string.IsNullOrWhiteSpace(report.Description))
                ModelState.AddModelError("Description", "Vui lòng nhập chú thích.");

            if (media == null || media.Length == 0)
                ModelState.AddModelError("MediaPath", "Bạn phải đính kèm hình ảnh hoặc video.");

            // Nếu có lỗi thì trả về View cùng dữ liệu nhập
            if (!ModelState.IsValid)
                return View(report);

            try
            {
                // Tạo thư mục nếu chưa có
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Lưu file
                var fileName = Path.GetFileName(media.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await media.CopyToAsync(stream);
                }

                // Gán thông tin còn thiếu
                report.ReporterId = userId.Value;
                report.CreatedAt = DateTime.Now;
                report.MediaPath = "/uploads/" + fileName;
                report.MediaType = media.ContentType.StartsWith("video") ? "video" : "image";

                // Lưu DB
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                // Thông báo thành công
                TempData["SuccessMessage"] = "Báo cáo đã được gửi thành công.";
                return RedirectToAction("ReportHistory");
            }
            catch (Exception ex)
            {
                // Báo lỗi nếu có vấn đề khi lưu file
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý file hoặc lưu dữ liệu: " + ex.Message);
                return View(report);
            }
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var report = _context.Reports
                .Include(r => r.Reporter)
                .FirstOrDefault(r => r.ReportId == id);

            if (report == null)
                return NotFound();

            return View("Detail", report); 
        }



        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var report = _context.Reports.Find(id);
            if (report == null)
                return NotFound();

            report.Status = report.Status == 1 ? 0 : 1;
            _context.SaveChanges();

            return RedirectToAction("ReportList");
        }
    }
}
