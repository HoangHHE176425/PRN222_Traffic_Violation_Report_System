using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Fine
{
    public int FineId { get; set; }

    public int? Amount { get; set; }

    public int Status { get; set; }

    public string IssuedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public int? ReportId { get; set; }

    public virtual ICollection<FineResponse> FineResponses { get; set; } = new List<FineResponse>();

    public virtual ICollection<FineViolationBehavior> FineViolationBehaviors { get; set; } = new List<FineViolationBehavior>();

    public virtual Vehicle? IssuedByNavigation { get; set; }

    public virtual Report? Report { get; set; }
}
