using Microsoft.AspNetCore.Mvc;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class VehicleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
