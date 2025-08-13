using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Cccd { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Address { get; set; }

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<FineResponse> FineResponses { get; set; } = new List<FineResponse>();

    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
