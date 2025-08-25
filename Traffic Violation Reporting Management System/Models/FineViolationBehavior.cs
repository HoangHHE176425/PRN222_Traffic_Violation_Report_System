using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class FineViolationBehavior
{
    public int Id { get; set; }

    public int FineId { get; set; }

    public int BehaviorId { get; set; }

    public int? DecidedAmount { get; set; }

    public virtual ViolationBehavior Behavior { get; set; } = null!;

    public virtual Fine Fine { get; set; } = null!;
}
