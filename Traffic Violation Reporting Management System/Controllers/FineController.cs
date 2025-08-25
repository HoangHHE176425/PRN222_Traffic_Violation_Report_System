using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System.DTOs;
using Traffic_Violation_Reporting_Management_System.Models;
using Traffic_Violation_Reporting_Management_System.Service;
using Traffic_Violation_Reporting_Management_System.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class FineController : Controller
    {
        private readonly TrafficViolationDbContext _context;
        private readonly SmsService _smsService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FineController(
            TrafficViolationDbContext context,
            SmsService smsService,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _smsService = smsService;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        [AuthorizeRole(1,2)]

        public IActionResult FineList(string search, int? status, string sortOrder, int page = 1, int pageSize = 10)
        {
            var query = _context.Fines.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(f => f.IssuedBy.Contains(search));

            if (status.HasValue)
                query = query.Where(f => f.Status == status);
            
            switch (sortOrder)
            {
                case "amount_asc": query = query.OrderBy(f => f.Amount); break;
                case "amount_desc": query = query.OrderByDescending(f => f.Amount); break;
                case "status_asc": query = query.OrderBy(f => f.Status); break;
                case "status_desc": query = query.OrderByDescending(f => f.Status); break;
                case "created_asc": query = query.OrderBy(f => f.CreatedAt); break;
                case "created_desc": query = query.OrderByDescending(f => f.CreatedAt); break;
                case "paid_asc": query = query.OrderBy(f => f.PaidAt); break;
                case "paid_desc": query = query.OrderByDescending(f => f.PaidAt); break;
                default: query = query.OrderByDescending(f => f.CreatedAt); break;
            }

            var pagedResult = query.GetPaged(page, pageSize);

            // Gửi dữ liệu sang View
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            return View("FineList", pagedResult);
        }
        [AuthorizeRole(0)]
        public IActionResult FineHistory(string search, int? status, string sortOrder, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login", "Auth");

            var userVehicleNumbers = _context.Vehicles
                .Where(v => v.OwnerCccd == user.Cccd)
                .Select(v => v.VehicleNumber)
                .ToList();

            var query = _context.Fines
                .Where(f => userVehicleNumbers.Contains(f.IssuedBy))
                .AsQueryable();

            // Bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(search))
                query = query.Where(f => f.IssuedBy.Contains(search));

            if (status.HasValue)
                query = query.Where(f => f.Status == status);

            // Sắp xếp
            switch (sortOrder)
            {
                case "amount_asc": query = query.OrderBy(f => f.Amount); break;
                case "amount_desc": query = query.OrderByDescending(f => f.Amount); break;
                case "status_asc": query = query.OrderBy(f => f.Status); break;
                case "status_desc": query = query.OrderByDescending(f => f.Status); break;
                case "created_asc": query = query.OrderBy(f => f.CreatedAt); break;
                case "created_desc": query = query.OrderByDescending(f => f.CreatedAt); break;
                case "paid_asc": query = query.OrderBy(f => f.PaidAt); break;
                case "paid_desc": query = query.OrderByDescending(f => f.PaidAt); break;
                default: query = query.OrderByDescending(f => f.CreatedAt); break;
            }

            // Phân trang
            var pagedResult = query.GetPaged(page, pageSize);

            // Giữ giá trị filter khi đổi trang
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            return View("FineHistory", pagedResult);
        }

        [AuthorizeRole(0,1,2)]

        public IActionResult Detail(int id)
        {
            var fine = _context.Fines
       .Include(f => f.FineViolationBehaviors)
           .ThenInclude(fvb => fvb.Behavior)
       .FirstOrDefault(f => f.FineId == id);


            if (fine == null) return NotFound();

            // Nếu có liên kết report, lấy media từ report
            if (fine.ReportId.HasValue)
            {
                var report = _context.Reports.FirstOrDefault(r => r.ReportId == fine.ReportId.Value);
                ViewBag.Report = report; 
            }


            return View(fine);
        }

        [AuthorizeRole(1,2)]

        public IActionResult Create(int? reportId)
        {
            ViewBag.ViolationBehaviors = _context.ViolationBehaviors
                .Select(v => new SelectListItem
                {
                    Value = v.BehaviorId.ToString(),
                    Text = v.Name
                }).ToList();

            ViewBag.BehaviorDetails = _context.ViolationBehaviors
                .Select(v => new
                {
                    behaviorId = v.BehaviorId,
                    name = v.Name,
                    minFineAmount = v.MinFineAmount,
                    maxFineAmount = v.MaxFineAmount
                }).ToList();

            if (reportId.HasValue)
            {
                var report = _context.Reports.FirstOrDefault(r => r.ReportId == reportId.Value);
                if (report != null)
                {
                    ViewBag.ReportId = report.ReportId;
                    ViewBag.MediaPath = report.MediaPath;
                    ViewBag.MediaType = report.MediaType;
                }
            }

            return View();
        }
        [AuthorizeRole(1,2)]

        [HttpPost]
        public async Task<IActionResult> Create(Fine fine, List<int> behaviorIds, List<decimal> amounts, int? reportId = null)
        {
            if (string.IsNullOrWhiteSpace(fine.IssuedBy))
            {
                ModelState.AddModelError("IssuedBy", "Vui lòng nhập biển số xe.");
            }
            else
            {
                // Kiểm tra xem biển số có tồn tại trong bảng Vehicle không
                var vehicle = _context.Vehicles.FirstOrDefault(v => v.VehicleNumber == fine.IssuedBy);
                if (vehicle == null)
                {
                    ModelState.AddModelError("IssuedBy", "Biển số xe không tồn tại trong hệ thống.");
                }
                else
                {
                    // Kiểm tra trạng thái của xe
                    if (vehicle.Status == 1) // Giả sử 1 = Tạm giam/Thu hồi
                    {
                        ModelState.AddModelError("IssuedBy", "Xe này hiện đã bị tạm giam hoặc thu hồi.");
                    }
                }
            }



            if (behaviorIds.Count != amounts.Count || behaviorIds.Count == 0)
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một hành vi vi phạm và nhập số tiền tương ứng.");

            if (!ModelState.IsValid)
            {
                ViewBag.ViolationBehaviors = _context.ViolationBehaviors
                    .Select(v => new SelectListItem { Value = v.BehaviorId.ToString(), Text = v.Name })
                    .ToList();
                return View(fine);
            }

            fine.CreatedAt = DateTime.Now;
            fine.Status = 0;
            fine.Amount = (int?)amounts.Sum();

            if (reportId.HasValue)
            {
                var report = _context.Reports.FirstOrDefault(r => r.ReportId == reportId.Value);
                if (report != null) fine.ReportId = report.ReportId;
            }

            _context.Fines.Add(fine);
            _context.SaveChanges();

            for (int i = 0; i < behaviorIds.Count; i++)
            {
                _context.FineViolationBehaviors.Add(new FineViolationBehavior
                {
                    FineId = fine.FineId,
                    BehaviorId = behaviorIds[i],
                    DecidedAmount = (int?)amounts[i]
                });
            }
            _context.SaveChanges();

            // Tìm chủ xe và gửi notification (dùng constructor của record)
            var user = _context.Users
                .FirstOrDefault(u => _context.Vehicles.Any(v => v.VehicleNumber == fine.IssuedBy && v.OwnerCccd == u.Cccd));
            if (user != null)
            {
                await _notificationService.CreateAsync(
                    new CreateNotificationRequest(
                        user.UserId,
                        "Fine",
                        "Phiếu phạt mới",
                        $"Bạn có phiếu phạt mới cho xe {fine.IssuedBy}.",
                        $"{{ \"fineId\": {fine.FineId} }}"
                    )
                );

                // 🚀 Bắn signalr tới group user này
                var unread = _context.Notifications.Count(n => n.UserId == user.UserId && !n.IsRead);
                await _hubContext.Clients.Group($"user:{user.UserId}")
                    .SendAsync("notify", new
                    {
                        counts = new { unread },
                        title = "Phiếu phạt mới",
                        message = $"Bạn có phiếu phạt mới cho xe {fine.IssuedBy}."
                    });
            }


            return RedirectToAction("FineList");
        }


        [AuthorizeRole(1,2)]

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var fine = _context.Fines.FirstOrDefault(f => f.FineId == id);
            if (fine == null) return NotFound();

            // Chỉ cho phép hủy nếu chưa thanh toán
            if (fine.Status == 0)
            {
                fine.Status = 2; // Đã hủy
                fine.PaidAt = DateTime.Now;
                _context.SaveChanges();

                var user = _context.Users
                    .FirstOrDefault(u => _context.Vehicles.Any(v => v.VehicleNumber == fine.IssuedBy && v.OwnerCccd == u.Cccd));
                if (user != null)
                {
                    await _notificationService.CreateAsync(
                        new CreateNotificationRequest(
                            user.UserId,
                            "Fine",
                            "Phiếu phạt đã hủy",
                            $"Phiếu phạt cho xe {fine.IssuedBy} đã bị hủy.",
                            $"{{ \"fineId\": {fine.FineId} }}"
                        )
                    );

                    var unread = _context.Notifications.Count(n => n.UserId == user.UserId && !n.IsRead);
                    await _hubContext.Clients.Group($"user:{user.UserId}")
                        .SendAsync("notify", new
                        {
                            counts = new { unread },
                            title = "Phiếu phạt đã hủy",
                            message = $"Phiếu phạt cho xe {fine.IssuedBy} đã bị hủy."
                        });
                }

            }

            return RedirectToAction("FineList");
        }

    }
}
