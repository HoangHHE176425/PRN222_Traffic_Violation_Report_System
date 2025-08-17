using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Traffic_Violation_Reporting_Management_System.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key] public int NotificationId { get; set; }
        [Required] public int UserId { get; set; }   
        [Required, MaxLength(50)] public string Type { get; set; } = default!;
        [Required, MaxLength(200)] public string Title { get; set; } = default!;
        [Required, MaxLength(500)] public string Message { get; set; } = default!;
        public string? DataJson { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
