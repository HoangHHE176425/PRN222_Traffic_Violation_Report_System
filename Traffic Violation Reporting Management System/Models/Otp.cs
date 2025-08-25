using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Otp
{
    public int OtpId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Otpcode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public string? Email { get; set; }

    public virtual User? EmailNavigation { get; set; }
}
