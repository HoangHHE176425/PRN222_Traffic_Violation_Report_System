using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
