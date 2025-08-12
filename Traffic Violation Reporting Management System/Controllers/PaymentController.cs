using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System.Models;
[AuthorizeRole(0)]

public class PaymentController : Controller
{
    private readonly TrafficViolationDbContext _context;
    private readonly PayOSService _payOS;
    private readonly IConfiguration _config;

    public PaymentController(TrafficViolationDbContext context, PayOSService payOS, IConfiguration config)
    {
        _context = context;
        _payOS = payOS;
        _config = config;
    }

    private int? GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId");
    }

    public async Task<IActionResult> PayFine(int fineId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var fine = await _context.Fines
            .Include(f => f.IssuedByNavigation)
            .FirstOrDefaultAsync(f => f.FineId == fineId);

        if (fine == null)
        {
            return NotFound();
        }

        // Explicitly cast fine.Amount to int and handle potential null values
        if (!fine.Amount.HasValue)
        {
            return BadRequest("Số tiền phạt không hợp lệ.");
        }
        int amount = fine.Amount.Value;

        var description = $"Phạt vi phạm #{fineId}";

        // Lấy thông tin người dùng từ Session
        string? userName = HttpContext.Session.GetString("FullName");
        string? userEmail = HttpContext.Session.GetString("Email");
        string? phone = fine.IssuedByNavigation?.OwnerPhoneNumber;

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(phone))
        {
            return BadRequest("Thiếu thông tin người dùng hoặc số điện thoại.");
        }

        // Tạo URL callback
        string baseUrl = _config["PayOS:BaseUrl"];
        string returnUrl = $"{baseUrl}/Payment/PaymentCallback?fineId={fine.FineId}&status=success";
        string cancelUrl = $"{baseUrl}/Payment/PaymentCallback?fineId={fine.FineId}&status=cancel";
        try
        {
            string paymentUrl = await _payOS.CreatePayment(fine.FineId, amount, description, returnUrl, phone, userName, userEmail);
            return Redirect(paymentUrl);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi và hiển thị thông báo
            Console.WriteLine("Lỗi tạo thanh toán: " + ex.Message);
            return BadRequest("Không thể tạo thanh toán.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> PaymentCallback(int fineId, string? status)
    {
        var fine = await _context.Fines.FindAsync(fineId);
        if (fine == null)
            return NotFound();

        if (status == "success" && fine.Status == 1)
        {
            ViewBag.Message = $"Thanh toán thành công cho biên bản #{fine.FineId}.";
        }
        else if (status == "cancel")
        {
            ViewBag.Message = $"Bạn đã hủy giao dịch thanh toán cho biên bản #{fine.FineId}.";
        }
        else
        {
            ViewBag.Message = $"Giao dịch chưa xác định hoặc chưa hoàn tất.";
        }

        return View("Success");
    }
    [HttpPost("api/payment/webhook")]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookModel payload)
    {
        var fine = await _context.Fines.FirstOrDefaultAsync(f => f.FineId == payload.OrderCode);
        if (fine == null)
            return NotFound();

        if (payload.Status == "PAID" && fine.Status != 1)
        {
            fine.Status = 1;
            fine.PaidAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }


}
