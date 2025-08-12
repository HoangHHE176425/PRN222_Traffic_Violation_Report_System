using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Traffic_Violation_Reporting_Management_System.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    [AuthorizeRole(2)]

    public class ViolationBehaviorController : Controller
    {
        private readonly TrafficViolationDbContext _context;

        public ViolationBehaviorController(TrafficViolationDbContext context)
        {
            _context = context;
        }

        public IActionResult ViolationBehaviorList(string searchName, decimal? minFine, decimal? maxFine)
        {
            var query = _context.ViolationBehaviors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(v => v.Name.Contains(searchName));
            }

            if (minFine.HasValue)
            {
                query = query.Where(v => v.MinFineAmount >= minFine.Value);
            }

            if (maxFine.HasValue)
            {
                query = query.Where(v => v.MaxFineAmount <= maxFine.Value);
            }

            ViewBag.SearchName = searchName;
            ViewBag.MinFine = minFine;
            ViewBag.MaxFine = maxFine;

            var result = query.OrderBy(v => v.MinFineAmount).ToList();
            return View(result);
        }

        // GET: /ViolationBehavior/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /ViolationBehavior/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ViolationBehavior behavior)
        {
            // RÀNG BUỘC THỦ CÔNG
            if (string.IsNullOrWhiteSpace(behavior.Name))
                ModelState.AddModelError(nameof(behavior.Name), "Tên lỗi vi phạm không được để trống");        
            if (!behavior.MinFineAmount.HasValue || behavior.MinFineAmount <= 0)
                ModelState.AddModelError(nameof(behavior.MinFineAmount), "Tiền phạt tối thiểu không được để trống và phải lớn hơn 0");

            if (!behavior.MaxFineAmount.HasValue || behavior.MaxFineAmount <= 0)
                ModelState.AddModelError(nameof(behavior.MaxFineAmount), "Tiền phạt tối đa không được để trống và phải lớn hơn 0");

            if (behavior.MinFineAmount.HasValue && behavior.MaxFineAmount.HasValue &&
                behavior.MinFineAmount >= behavior.MaxFineAmount)
                ModelState.AddModelError(nameof(behavior.MinFineAmount), "Tiền phạt tối thiểu phải nhỏ hơn tiền phạt tối đa");

            if (!ModelState.IsValid)
            {
                return View(behavior);
            }

            try
            {
                _context.ViolationBehaviors.Add(behavior);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Thêm hành vi vi phạm thành công.";
                return RedirectToAction("ViolationBehaviorList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi thêm hành vi: " + ex.Message);
                return View(behavior);
            }
        }



        // GET: /ViolationBehavior/Detail/5
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var behavior = _context.ViolationBehaviors.FirstOrDefault(v => v.BehaviorId == id);
            if (behavior == null)
                return NotFound();

            return View(behavior);
        }

        // POST: /ViolationBehavior/EditDetail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDetail(ViolationBehavior updated)
        {
            var existing = _context.ViolationBehaviors.FirstOrDefault(v => v.BehaviorId == updated.BehaviorId);
            if (existing == null)
                return NotFound();

            // RÀNG BUỘC THỦ CÔNG
            if (string.IsNullOrWhiteSpace(updated.Name))
                ModelState.AddModelError(nameof(updated.Name), "Tên lỗi vi phạm không được để trống");

            if (!updated.MinFineAmount.HasValue || updated.MinFineAmount <= 0)
                ModelState.AddModelError(nameof(updated.MinFineAmount), "Tiền phạt tối thiểu không được để trống và phải lớn hơn 0");

            if (!updated.MaxFineAmount.HasValue || updated.MaxFineAmount <= 0)
                ModelState.AddModelError(nameof(updated.MaxFineAmount), "Tiền phạt tối đa không được để trống và phải lớn hơn 0");

            if (updated.MinFineAmount.HasValue && updated.MaxFineAmount.HasValue &&
                updated.MinFineAmount >= updated.MaxFineAmount)
                ModelState.AddModelError(nameof(updated.MinFineAmount), "Tiền phạt tối thiểu phải nhỏ hơn tiền phạt tối đa");

            if (!ModelState.IsValid)
            {
                return View("Detail", updated);
            }

            existing.Name = updated.Name;
            existing.MinFineAmount = updated.MinFineAmount;
            existing.MaxFineAmount = updated.MaxFineAmount;

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật hành vi vi phạm thành công.";
            return RedirectToAction("Detail", new { id = updated.BehaviorId });
        }


    }
}
