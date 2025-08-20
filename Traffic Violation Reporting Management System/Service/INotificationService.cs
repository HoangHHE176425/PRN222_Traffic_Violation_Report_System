using Traffic_Violation_Reporting_Management_System.DTOs;

namespace Traffic_Violation_Reporting_Management_System.Service
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateAsync(CreateNotificationRequest req);
        Task<IReadOnlyList<NotificationDto>> GetAsync(int userId, bool onlyUnread, int page, int pageSize);
        Task<int> UnreadCountAsync(int userId);
        Task<int> TotalCountAsync(int userId);
        Task<(int total, int unread)> GetCountsAsync(int userId);
        Task<bool> MarkReadAsync(int userId, int notificationId);
        Task<int> MarkAllReadAsync(int userId);
    }
}
