using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class FineResponseController : Controller
    {
        private readonly TrafficViolationDbContext _context;

        public FineResponseController(TrafficViolationDbContext context)
        {
            _context = context;
        }
        [AuthorizeRole(0)]

        // Người dùng tạo khiếu nại
        public IActionResult Create(int fineId)
        {
            var fine = _context.Fines.Find(fineId);
            if (fine == null) return NotFound();

            var response = new FineResponse
            {
                FineId = fineId
            };

            return View(response);
        }

        // POST: FineResponse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(0)]

        public async Task<IActionResult> Create(FineResponse response, IFormFile media)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                // RÀNG BUỘC: Nội dung bắt buộc
                if (string.IsNullOrWhiteSpace(response.Content))
                    ModelState.AddModelError("Content", "Vui lòng nhập nội dung khiếu nại.");

                // RÀNG BUỘC: File nếu có thì kiểm tra loại và dung lượng
                if (media != null && media.Length > 0)
                {
                    // 100MB
                    if (media.Length > 100 * 1024 * 1024)
                        ModelState.AddModelError("media", "Tệp đính kèm không được vượt quá 100MB.");

                    // Kiểm tra định dạng hợp lệ
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".avi" };
                    var extension = Path.GetExtension(media.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                        ModelState.AddModelError("media", "Chỉ cho phép các tệp ảnh/video (.jpg, .png, .mp4, ...).");
                    ModelState.Remove("Fine");
                    ModelState.Remove("User");

                    if (!ModelState.IsValid)
                    {
                        return View(response);
                    }

                    if (ModelState.IsValid)
                    {
                        var fileName = Guid.NewGuid() + extension;
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await media.CopyToAsync(stream);
                        }

                        response.MediaPath = "/uploads/" + fileName;
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(response);
                }

                response.UserId = userId.Value;
                response.Status = 0;
                response.CreatedAt = DateTime.Now;

                _context.FineResponses.Add(response);
                await _context.SaveChangesAsync();

                return RedirectToAction("FineHistory", "Fine");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi gửi khiếu nại: " + ex.Message;
                return View(response);
            }
        }


        // Cảnh sát xem danh sách khiếu nại
        // Cảnh sát xem danh sách tất cả khiếu nại
        [AuthorizeRole(1,2)]

        public IActionResult FineResponseList(string search, int? status)
        {
            var query = _context.FineResponses
                .Include(r => r.Fine)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.Fine.IssuedBy.Contains(search) || r.User.FullName.Contains(search));

            if (status.HasValue)
                query = query.Where(r => r.Status == status);

            var result = query.OrderByDescending(r => r.CreatedAt).ToList();
            return View("FineResponseList", result); // View riêng cho cảnh sát
        }

        // Người dùng xem lịch sử khiếu nại của mình
        [AuthorizeRole(0)]

        public IActionResult FineResponseHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var responses = _context.FineResponses
                .Include(r => r.Fine)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View("FineResponseHistory", responses); // View riêng cho người dùng
        }
        [AuthorizeRole(0,1,2)]


        // Chi tiết khiếu nại
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var response = _context.FineResponses
                .Include(r => r.Fine)
                    .ThenInclude(f => f.Report) // include Report thông qua Fine
                .Include(r => r.User)
                .FirstOrDefault(r => r.ResponseId == id);

            if (response == null) return NotFound();

            var role = HttpContext.Session.GetInt32("Role");
            ViewBag.IsPolice = (role == 1);
            ViewBag.Report = response.Fine?.Report;

            return View(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(1,2)]

        public async Task<IActionResult> Reply(int id, string replyContent)
        {
            var response = _context.FineResponses.FirstOrDefault(r => r.ResponseId == id);
            if (response == null) return NotFound();

            response.Reply = replyContent;
            response.Status = 1; // Đã phản hồi
            response.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // 👉 Quay về trang danh sách phản hồi (action FineResponseList)
            return RedirectToAction("FineResponseList", "FineResponse");
        }


    }
}
