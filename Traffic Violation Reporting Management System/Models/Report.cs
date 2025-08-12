using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int ReporterId { get; set; }

    public string? Location { get; set; }

    public DateTime? TimeOfViolation { get; set; }

    public string? Description { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string MediaPath { get; set; } = null!;

    public string? MediaType { get; set; }

    public string? Comment { get; set; }

    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();

    public virtual User Reporter { get; set; } = null!;
}
