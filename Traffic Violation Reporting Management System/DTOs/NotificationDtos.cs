using System;

namespace Traffic_Violation_Reporting_Management_System.DTOs
{
    public record NotificationDto(
        int NotificationId,
        int UserId,
        string Type,
        string Title,
        string Message,
        string? DataJson,
        bool IsRead,
        DateTime CreatedAtUtc
    );

    public record CreateNotificationRequest(
        int UserId,
        string Type,
        string Title,
        string Message,
        string? DataJson
    );
}
