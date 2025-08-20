using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Complaint
{
    public int ComplaintId { get; set; }

    public int UserId { get; set; }

    public int? ReportId { get; set; }

    public string? Content { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ComplaintResponse> ComplaintResponses { get; set; } = new List<ComplaintResponse>();

    public virtual Report? Report { get; set; }

    public virtual User User { get; set; } = null!;
}
