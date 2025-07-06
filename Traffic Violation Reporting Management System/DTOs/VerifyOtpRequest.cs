using System.ComponentModel.DataAnnotations;

namespace Traffic_Violation_Reporting_Management_System.DTOs
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải là 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = string.Empty;
    }
} 