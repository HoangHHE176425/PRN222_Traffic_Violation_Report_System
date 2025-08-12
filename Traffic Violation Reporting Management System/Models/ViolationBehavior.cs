using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class ViolationBehavior
{
    public int BehaviorId { get; set; }

    public string? Name { get; set; } 

    public int? MinFineAmount { get; set; }

    public int? MaxFineAmount { get; set; }

    public virtual ICollection<FineViolationBehavior> FineViolationBehaviors { get; set; } = new List<FineViolationBehavior>();
}
