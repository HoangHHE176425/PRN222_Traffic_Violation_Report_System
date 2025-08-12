using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    [AuthorizeRole(1, 2)]

    public class VehicleController : Controller
    {
        private readonly TrafficViolationDbContext _context;

        public VehicleController(TrafficViolationDbContext context)
        {
            _context = context;
        }
        public IActionResult VehicleList(
      string search,
      string brandFilter,
      string modelFilter,
      string colorFilter,
      string statusFilter,
      string sortOrder)
        {
            var query = _context.Vehicles.AsQueryable();

            // Tìm kiếm theo biển số, chủ xe, số khung
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v =>
                    v.VehicleNumber.Contains(search) ||
                    v.OwnerName.Contains(search) ||
                    v.ChassicNo.Contains(search));
            }

            // Lọc theo hãng
            if (!string.IsNullOrWhiteSpace(brandFilter))
            {
                query = query.Where(v => v.Brand == brandFilter);
            }

            // Lọc theo dòng xe (model)
            if (!string.IsNullOrWhiteSpace(modelFilter))
            {
                query = query.Where(v => v.Model == modelFilter);
            }

            // Lọc theo màu xe
            if (!string.IsNullOrWhiteSpace(colorFilter))
            {
                query = query.Where(v => v.Color == colorFilter);
            }

            // Lọc theo trạng thái (int)
            if (!string.IsNullOrWhiteSpace(statusFilter) && int.TryParse(statusFilter, out int statusInt))
            {
                query = query.Where(v => v.Status == statusInt);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(v => v.RegistrationDate);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(v => v.RegistrationDate);
                    break;
                default:
                    query = query.OrderByDescending(v => v.RegistrationDate); // mặc định
                    break;
            }

            // Gán các filter hiện tại về lại ViewBag để giữ trạng thái chọn
            ViewBag.SelectedBrand = brandFilter;
            ViewBag.SelectedModel = modelFilter;
            ViewBag.SelectedColor = colorFilter;
            ViewBag.SelectedStatus = statusFilter;
            ViewBag.SelectedSortOrder = sortOrder;
            ViewBag.Search = search;

            // Gán danh sách để hiển thị trong dropdown
            ViewBag.Brands = _context.Vehicles
                .Select(v => v.Brand)
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Models = _context.Vehicles
                .Select(v => v.Model)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Colors = _context.Vehicles
                .Select(v => v.Color)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var result = query.ToList();
            return View("VehicleList", result);
        }

        [HttpPost]
        public IActionResult ChangeStatus(int id, int newStatus)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null)
                return NotFound();

            vehicle.Status = newStatus;
            _context.SaveChanges();

            return RedirectToAction("VehicleList");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Vehicle vehicle)
        {
            // 1. Kiểm tra không để trống và định dạng hợp lệ

            // Chủ xe
            if (string.IsNullOrWhiteSpace(vehicle.OwnerName))
                ModelState.AddModelError("OwnerName", "Tên chủ xe không được để trống.");

            // CCCD
            if (string.IsNullOrWhiteSpace(vehicle.OwnerCccd))
                ModelState.AddModelError("OwnerCccd", "CCCD không được để trống.");
            else if (!Regex.IsMatch(vehicle.OwnerCccd, @"^\d{12}$"))
                ModelState.AddModelError("OwnerCccd", "CCCD phải gồm đúng 12 chữ số.");
            else if (_context.Vehicles.Any(v => v.OwnerCccd == vehicle.OwnerCccd))
                ModelState.AddModelError("OwnerCccd", "CCCD đã tồn tại.");

            // SĐT
            if (string.IsNullOrWhiteSpace(vehicle.OwnerPhoneNumber))
                ModelState.AddModelError("OwnerPhoneNumber", "Số điện thoại không được để trống.");
            else if (!Regex.IsMatch(vehicle.OwnerPhoneNumber, @"^(03|05|07|08|09)\d{8}$"))
                ModelState.AddModelError("OwnerPhoneNumber", "SĐT phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và gồm 10 số.");
            else if (_context.Vehicles.Any(v => v.OwnerPhoneNumber == vehicle.OwnerPhoneNumber))
                ModelState.AddModelError("OwnerPhoneNumber", "Số điện thoại đã tồn tại.");

            // Địa chỉ, hãng, dòng, màu xe
            if (string.IsNullOrWhiteSpace(vehicle.Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống.");
            if (string.IsNullOrWhiteSpace(vehicle.Brand))
                ModelState.AddModelError("Brand", "Hãng xe không được để trống.");
            if (string.IsNullOrWhiteSpace(vehicle.Model))
                ModelState.AddModelError("Model", "Dòng xe không được để trống.");
            if (string.IsNullOrWhiteSpace(vehicle.Color))
                ModelState.AddModelError("Color", "Màu xe không được để trống.");

            // Biển số, khung, máy
            if (string.IsNullOrWhiteSpace(vehicle.VehicleNumber))
                ModelState.AddModelError("VehicleNumber", "Biển số xe không được để trống.");
            else if (_context.Vehicles.Any(v => v.VehicleNumber == vehicle.VehicleNumber))
                ModelState.AddModelError("VehicleNumber", "Biển số xe đã tồn tại.");

            if (string.IsNullOrWhiteSpace(vehicle.ChassicNo))
                ModelState.AddModelError("ChassicNo", "Số khung không được để trống.");
            else if (_context.Vehicles.Any(v => v.ChassicNo == vehicle.ChassicNo))
                ModelState.AddModelError("ChassicNo", "Số khung đã tồn tại.");

            if (string.IsNullOrWhiteSpace(vehicle.EngineNo))
                ModelState.AddModelError("EngineNo", "Số máy không được để trống.");
            else if (_context.Vehicles.Any(v => v.EngineNo == vehicle.EngineNo))
                ModelState.AddModelError("EngineNo", "Số máy đã tồn tại.");

            // Ngày đăng ký
            if (!vehicle.RegistrationDate.HasValue)
                ModelState.AddModelError("RegistrationDate", "Ngày đăng ký không được để trống.");
            else if (vehicle.RegistrationDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("RegistrationDate", "Ngày đăng ký không được lớn hơn hiện tại.");

            // 2. Kiểm tra hợp lệ tổng thể
            if (!ModelState.IsValid)
                return View(vehicle);

            // 3. Lưu vào DB
            try
            {
                vehicle.Status = 0;
                _context.Vehicles.Add(vehicle);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Thêm phương tiện mới thành công.";
                return RedirectToAction("Detail", new { id = vehicle.VehicleId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm phương tiện: " + ex.Message);
                if (ex.InnerException != null)
                    ModelState.AddModelError("", "Chi tiết: " + ex.InnerException.Message);
                return View(vehicle);
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }



        [HttpPost]
        public IActionResult EditDetail(Vehicle updated)
        {
            var existing = _context.Vehicles.FirstOrDefault(v => v.VehicleId == updated.VehicleId);
            if (existing == null)
                return NotFound();

            // ===== 1. Kiểm tra các trường =====
            if (string.IsNullOrWhiteSpace(updated.OwnerName))
                ModelState.AddModelError("OwnerName", "Tên chủ xe không được để trống.");

            if (string.IsNullOrWhiteSpace(updated.OwnerCccd))
                ModelState.AddModelError("OwnerCccd", "CCCD không được để trống.");
            else if (!Regex.IsMatch(updated.OwnerCccd, @"^\d{12}$"))
                ModelState.AddModelError("OwnerCccd", "CCCD phải gồm đúng 12 chữ số.");
            else if (_context.Vehicles.Any(v => v.OwnerCccd == updated.OwnerCccd && v.VehicleId != updated.VehicleId))
                ModelState.AddModelError("OwnerCccd", "CCCD đã tồn tại.");

            if (string.IsNullOrWhiteSpace(updated.OwnerPhoneNumber))
                ModelState.AddModelError("OwnerPhoneNumber", "Số điện thoại không được để trống.");
            else if (!Regex.IsMatch(updated.OwnerPhoneNumber, @"^(03|05|07|08|09)\d{8}$"))
                ModelState.AddModelError("OwnerPhoneNumber", "SĐT phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và gồm 10 số.");
            else if (_context.Vehicles.Any(v => v.OwnerPhoneNumber == updated.OwnerPhoneNumber && v.VehicleId != updated.VehicleId))
                ModelState.AddModelError("OwnerPhoneNumber", "Số điện thoại đã tồn tại.");

            if (string.IsNullOrWhiteSpace(updated.Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống.");
            if (string.IsNullOrWhiteSpace(updated.Brand))
                ModelState.AddModelError("Brand", "Hãng xe không được để trống.");
            if (string.IsNullOrWhiteSpace(updated.Model))
                ModelState.AddModelError("Model", "Dòng xe không được để trống.");
            if (string.IsNullOrWhiteSpace(updated.Color))
                ModelState.AddModelError("Color", "Màu xe không được để trống.");

   

            if (string.IsNullOrWhiteSpace(updated.ChassicNo))
                ModelState.AddModelError("ChassicNo", "Số khung không được để trống.");
            else if (_context.Vehicles.Any(v => v.ChassicNo == updated.ChassicNo && v.VehicleId != updated.VehicleId))
                ModelState.AddModelError("ChassicNo", "Số khung đã tồn tại.");

            if (string.IsNullOrWhiteSpace(updated.EngineNo))
                ModelState.AddModelError("EngineNo", "Số máy không được để trống.");
            else if (_context.Vehicles.Any(v => v.EngineNo == updated.EngineNo && v.VehicleId != updated.VehicleId))
                ModelState.AddModelError("EngineNo", "Số máy đã tồn tại.");

            if (!updated.RegistrationDate.HasValue)
                ModelState.AddModelError("RegistrationDate", "Ngày đăng ký không được để trống.");
            else if (updated.RegistrationDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("RegistrationDate", "Ngày đăng ký không được lớn hơn hiện tại.");

            // ===== 2. Trả về nếu có lỗi =====
            if (!ModelState.IsValid)
                return View("Detail", updated);

            // ===== 3. Gán và lưu dữ liệu =====
            existing.OwnerName = updated.OwnerName;
            existing.OwnerCccd = updated.OwnerCccd;
            existing.OwnerPhoneNumber = updated.OwnerPhoneNumber;
            existing.Address = updated.Address;
            existing.ChassicNo = updated.ChassicNo;
            existing.EngineNo = updated.EngineNo;
            existing.Brand = updated.Brand;
            existing.Model = updated.Model;
            existing.Color = updated.Color;
            existing.RegistrationDate = updated.RegistrationDate;
            existing.Status = updated.Status;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật thông tin phương tiện thành công.";
            return RedirectToAction("Detail", new { id = updated.VehicleId });
        }



    }
}
    
