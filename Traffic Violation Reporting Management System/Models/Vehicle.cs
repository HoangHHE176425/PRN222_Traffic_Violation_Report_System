using System;
using System.Collections.Generic;

namespace Traffic_Violation_Reporting_Management_System.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public string? OwnerCccd { get; set; } 

    public string? OwnerName { get; set; }

    public string? OwnerPhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? VehicleNumber { get; set; } 

    public string? ChassicNo { get; set; }

    public string? EngineNo { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? Color { get; set; }

    public DateOnly? RegistrationDate { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();
}
