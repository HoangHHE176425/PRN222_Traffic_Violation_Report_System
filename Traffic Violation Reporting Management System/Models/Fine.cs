using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Fine
{
    public int FineId { get; set; }

    public int ReportId { get; set; }

    public decimal? Amount { get; set; }

    public int? Status { get; set; }

    public string IssuedBy { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Vehicle IssuedByNavigation { get; set; } = null!;

    public virtual Report Report { get; set; } = null!;
}
