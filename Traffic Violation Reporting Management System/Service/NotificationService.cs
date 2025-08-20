using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Traffic_Violation_Reporting_Management_System.DTOs;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Service
{
    public class NotificationService : INotificationService
    {
        private readonly TrafficViolationDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            TrafficViolationDbContext db,
            IHubContext<NotificationHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ================== CREATE ==================
        public async Task<NotificationDto> CreateAsync(CreateNotificationRequest req)
        {
            var n = new Notification
            {
                UserId = req.UserId,
                Type = req.Type,
                Title = req.Title,
                Message = req.Message,
                DataJson = req.DataJson,
                // Nếu cột CreatedAt có default GETDATE() ở DB thì có thể bỏ dòng dưới
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();

            var dto = ToDto(n);

            // Bắn realtime; nếu lỗi mạng thì log nhưng KHÔNG rollback DB
            try
            {
                // lấy counts để front-end cập nhật badge chính xác
                var counts = await GetCountsAsync(req.UserId);

                await _hub.Clients.Group($"user:{req.UserId}")
                    .SendAsync("notify", new
                    {
                        notification = dto,
                        counts = new { total = counts.total, unread = counts.unread }
                    });
            }
            catch
            {
                // TODO: log warning nếu cần
            }

            return dto;
        }

        // ================== QUERY LIST ==================
        public async Task<IReadOnlyList<NotificationDto>> GetAsync(int userId, bool onlyUnread, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var q = _db.Notifications.AsNoTracking().Where(x => x.UserId == userId);
            if (onlyUnread) q = q.Where(x => !x.IsRead);

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.NotificationId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationDto(
                    x.NotificationId, x.UserId, x.Type, x.Title, x.Message, x.DataJson, x.IsRead, x.CreatedAt
                ))
                .ToListAsync();

            return items;
        }

        // ================== COUNTS ==================
        public async Task<int> UnreadCountAsync(int userId)
            => await _db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);

        // >>> Thêm mới: tổng số thông báo
        public async Task<int> TotalCountAsync(int userId)
            => await _db.Notifications.CountAsync(x => x.UserId == userId);

        // >>> Tiện dụng: lấy cả total & unread
        public async Task<(int total, int unread)> GetCountsAsync(int userId)
        {
            // chạy 2 count song song cho nhanh
            var totalTask = _db.Notifications.CountAsync(x => x.UserId == userId);
            var unreadTask = _db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);
            await Task.WhenAll(totalTask, unreadTask);
            return (await totalTask, await unreadTask);
        }

        // ================== UPDATE READ STATE ==================
        public async Task<bool> MarkReadAsync(int userId, int notificationId)
        {
            var n = await _db.Notifications
                .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

            if (n is null) return false;

            if (!n.IsRead)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();

                // có thể bắn counts để front-end cập nhật ngay
                await TryBroadcastCounts(userId);
            }
            return true;
        }

        public async Task<int> MarkAllReadAsync(int userId)
        {
            var list = await _db.Notifications
                                .Where(x => x.UserId == userId && !x.IsRead)
                                .ToListAsync();

            if (list.Count == 0) return 0;

            foreach (var n in list)
                n.IsRead = true;

            await _db.SaveChangesAsync();

            // có thể bắn counts để front-end cập nhật ngay
            await TryBroadcastCounts(userId);

            return list.Count;
        }

        // ================== HELPERS ==================
        private async Task TryBroadcastCounts(int userId)
        {
            try
            {
                var counts = await GetCountsAsync(userId);
                await _hub.Clients.Group($"user:{userId}")
                    .SendAsync("notifyCounts", new { total = counts.total, unread = counts.unread });
            }
            catch
            {
                // ignore; optional log
            }
        }

        private static NotificationDto ToDto(Notification n) =>
            new(n.NotificationId, n.UserId, n.Type, n.Title, n.Message, n.DataJson, n.IsRead, n.CreatedAt);
    }
}
