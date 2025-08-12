using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Traffic_Violation_Reporting_Management_System.Models;
using Traffic_Violation_Reporting_Management_System.ViewModels;
[AuthorizeRole(2)]
public class DashboardController : Controller
{
    private readonly TrafficViolationDbContext _context;

    public DashboardController(TrafficViolationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Tổng số lượng
        var totalUsers = await _context.Users.CountAsync();
        var totalVehicles = await _context.Vehicles.CountAsync();
        var totalFines = await _context.Fines.CountAsync();
        var totalTransactions = await _context.Transactions.CountAsync();
        var totalReports = await _context.Reports.CountAsync();
        var totalFineResponses = await _context.FineResponses.CountAsync();

        var cards = new List<StatisticCardModel>
        {
            new StatisticCardModel { Title = "Người dùng", Value = totalUsers, Color = "primary", Icon = "bi-person-check" },
            new StatisticCardModel { Title = "Phương tiện", Value = totalVehicles, Color = "success", Icon = "bi-car-front" },
            new StatisticCardModel { Title = "Lỗi vi phạm", Value = totalFines, Color = "warning", Icon = "bi-exclamation-circle" },
            new StatisticCardModel { Title = "Giao dịch", Value = totalTransactions, Color = "info", Icon = "bi-currency-exchange" },
            new StatisticCardModel { Title = "Báo cáo", Value = totalReports, Color = "danger", Icon = "bi-flag" },
            new StatisticCardModel { Title = "Phản hồi xử phạt", Value = totalFineResponses, Color = "secondary", Icon = "bi-reply-all" }
        };

        // Lỗi theo hành vi
        var finesByBehavior = await _context.FineViolationBehaviors
            .Include(fvb => fvb.Behavior)
            .GroupBy(fvb => fvb.Behavior.Name)
            .Select(g => new BehaviorCountModel
            {
                Behavior = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Biểu đồ trạng thái lỗi
        var fineStatus = await _context.Fines
            .GroupBy(f => f.Status)
            .ToDictionaryAsync(
                g => g.Key.HasValue ? g.Key.Value.ToString() : "Không xác định",
                g => g.Count()
            );

        // Biểu đồ trạng thái báo cáo
        var reportStatus = await _context.Reports
            .GroupBy(r => r.Status)
            .ToDictionaryAsync(
                g => g.Key.HasValue ? g.Key.Value.ToString() : "Không xác định",
                g => g.Count()
            );

        // Biểu đồ trạng thái phản hồi xử phạt
        var fineResponseStatus = await _context.FineResponses
            .GroupBy(r => r.Status)
            .ToDictionaryAsync(
                g => g.Key.HasValue ? g.Key.Value.ToString() : "Không xác định",
                g => g.Count()
            );

        var viewModel = new DashboardViewModel
        {
            StatisticCards = cards,
            FinesByBehavior = finesByBehavior,
            FineStatusChart = fineStatus,
            ReportStatusChart = reportStatus,
            FineResponseStatusChart = fineResponseStatus
        };

        return View(viewModel);
    }
}
