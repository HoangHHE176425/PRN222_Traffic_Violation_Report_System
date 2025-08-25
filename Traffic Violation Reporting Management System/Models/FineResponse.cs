using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class FineResponse
{
    public int ResponseId { get; set; }

    public int FineId { get; set; }

    public int UserId { get; set; }

    public string? Content { get; set; }

    public string? MediaPath { get; set; }

    public DateTime CreatedAt { get; set; }

    public int Status { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Reply { get; set; }

    public virtual Fine Fine { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
