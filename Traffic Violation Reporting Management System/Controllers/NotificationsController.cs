using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Traffic_Violation_Reporting_Management_System.DTOs;
using Traffic_Violation_Reporting_Management_System.Service;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _svc;
        public NotificationsController(INotificationService svc) => _svc = svc;

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(idStr))
            {
                var sid = HttpContext?.Session?.GetInt32("UserId");
                if (sid.HasValue) { userId = sid.Value; return true; }
            }
            return int.TryParse(idStr, out userId);
        }

        private static (int page, int pageSize) Normalize(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;
            return (page, pageSize);
        }

        // ===== MVC VIEW =====
        // GET /Notifications
        [HttpGet("/Notifications")]
        public async Task<IActionResult> Index(bool onlyUnread = false, int page = 1, int pageSize = 20)
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized();

            (page, pageSize) = Normalize(page, pageSize);
            var items = await _svc.GetAsync(uid, onlyUnread, page, pageSize);

            ViewBag.OnlyUnread = onlyUnread;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Unread = await _svc.UnreadCountAsync(uid);

            return View(items); // Views/Notifications/Index.cshtml
        }

        // POST /Notifications/MarkRead
        [HttpPost("/Notifications/MarkRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id, bool onlyUnread, int page, int pageSize)
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized();
            await _svc.MarkReadAsync(uid, id);
            return RedirectToAction(nameof(Index), new { onlyUnread, page, pageSize });
        }

        // POST /Notifications/MarkAllRead
        [HttpPost("/Notifications/MarkAllRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead(bool onlyUnread, int page, int pageSize)
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized();
            await _svc.MarkAllReadAsync(uid);
            return RedirectToAction(nameof(Index), new { onlyUnread, page, pageSize });
        }

        // ===== API =====
        // GET /api/notifications
        [HttpGet("/api/notifications")]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromQuery] bool onlyUnread = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized(new { message = "Không xác định được người dùng." });
            (page, pageSize) = Normalize(page, pageSize);
            var list = await _svc.GetAsync(uid, onlyUnread, page, pageSize);
            return Ok(list);
        }

        // GET /api/notifications/unread-count
        [HttpGet("/api/notifications/unread-count")]
        [Produces("application/json")]
        public async Task<IActionResult> UnreadCount()
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized(new { message = "Không xác định được người dùng." });
            var c = await _svc.UnreadCountAsync(uid);
            return Ok(new { unread = c });
        }

        // PATCH /api/notifications/{id}/read
        [HttpPatch("/api/notifications/{id:int}/read")]
        [Produces("application/json")]
        public async Task<IActionResult> ApiMarkRead([FromRoute] int id)
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized(new { message = "Không xác định được người dùng." });
            var ok = await _svc.MarkReadAsync(uid, id);
            return ok ? NoContent() : NotFound();
        }

        // PATCH /api/notifications/read-all
        [HttpPatch("/api/notifications/read-all")]
        [Produces("application/json")]
        public async Task<IActionResult> ApiMarkAllRead()
        {
            if (!TryGetCurrentUserId(out var uid)) return Unauthorized(new { message = "Không xác định được người dùng." });
            var n = await _svc.MarkAllReadAsync(uid);
            return Ok(new { updated = n });
        }

        // POST /api/notifications
        [HttpPost("/api/notifications")]
        [Authorize(Roles = "Admin,Officer")]
        [Produces("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateNotificationRequest req)
        {
            if (req is null) return BadRequest(new { message = "Body rỗng." });
            if (req.UserId <= 0 || string.IsNullOrWhiteSpace(req.Type) ||
                string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { message = "Thiếu trường bắt buộc (userId, type, title, message)." });

            var dto = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id = dto.NotificationId }, dto);
        }

        // GET /api/notifications/count
        [HttpGet("/api/notifications/count")]
        [Produces("application/json")]
        public async Task<IActionResult> CountAll()
        {
            if (!TryGetCurrentUserId(out var uid))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var total = await _svc.TotalCountAsync(uid);
            var unread = await _svc.UnreadCountAsync(uid);
            return Ok(new { total, unread });
        }

    }
}
