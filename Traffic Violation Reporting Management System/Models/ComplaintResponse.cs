using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class ComplaintResponse
{
    public int ResponseId { get; set; }

    public int ComplaintId { get; set; }

    public int ResponderId { get; set; }

    public string? ResponseText { get; set; }

    public DateTime? RespondedAt { get; set; }

    public virtual Complaint Complaint { get; set; } = null!;

    public virtual User Responder { get; set; } = null!;
}
