using System.Collections.Generic;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.ViewModels
{
    public class DashboardViewModel
    {
        public List<StatisticCardModel> StatisticCards { get; set; }
        public List<BehaviorCountModel> FinesByBehavior { get; set; }
        public Dictionary<string, int> FineStatusChart { get; set; }
        public Dictionary<string, int> ReportStatusChart { get; set; }
        public Dictionary<string, int> FineResponseStatusChart { get; set; } // Đã giữ lại 1 bản đúng
    }

    public class StatisticCardModel
    {
        public string Title { get; set; }
        public int Value { get; set; }
        public string Color { get; set; } // ví dụ: "primary", "danger"
        public string Icon { get; set; }  // ví dụ: "bi-people", "bi-car-front"
    }

    public class BehaviorCountModel
    {
        public string Behavior { get; set; }
        public int Count { get; set; }
    }
}
