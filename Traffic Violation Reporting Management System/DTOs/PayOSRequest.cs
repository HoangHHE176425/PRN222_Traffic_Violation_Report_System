namespace Traffic_Violation_Reporting_Management_System.DTOs
{
    public class PayOSRequest
    {
        public int OrderCode { get; set; }
        public int Amount { get; set; } // PayOS expects int
        public string Description { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } // optional
    }
}
