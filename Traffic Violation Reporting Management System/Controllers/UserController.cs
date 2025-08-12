using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Traffic_Violation_Reporting_Management_System.Models;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    [AuthorizeRole(2)]

    public class UserController : Controller
    {
        private readonly TrafficViolationDbContext _context;
        public UserController(TrafficViolationDbContext context)
        {
             _context = context;
        }
        public IActionResult UserList(string search, string roleFilter, string statusFilter, string sortOrder)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Cccd.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.PhoneNumber.Contains(search) ||
                    u.Address.Contains(search));
            }

            if (!string.IsNullOrEmpty(roleFilter) && int.TryParse(roleFilter, out int role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrEmpty(statusFilter) && bool.TryParse(statusFilter, out bool isActive))
            {
                query = query.Where(u => u.IsActive == isActive);
            }

            switch (sortOrder)
            {
                case "created_asc":
                    query = query.OrderBy(u => u.CreatedAt);
                    break;
                case "created_desc":
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(u => u.Role);
                    break;
            }

            ViewBag.SelectedSearch = search;
            ViewBag.SelectedRole = roleFilter;
            ViewBag.SelectedStatus = statusFilter;
            ViewBag.SelectedSort = sortOrder;

            return View(query.ToList());
        }
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

        [HttpPost]
        public IActionResult ChangeStatus(int id, bool newStatus)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsActive = newStatus;
            _context.SaveChanges();

            return RedirectToAction("UserList");
        }
        public IActionResult Detail(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View("Detail", user); // <<< gọi view Detail
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(User updatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == updatedUser.UserId);
            if (user == null) return NotFound();

            // Validate thủ công
            if (string.IsNullOrWhiteSpace(updatedUser.FullName))
                ModelState.AddModelError("FullName", "Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(updatedUser.PhoneNumber))
                ModelState.AddModelError("PhoneNumber", "SĐT không được để trống.");
            else if (!Regex.IsMatch(updatedUser.PhoneNumber, @"^(03|05|07|08|09)\d{8}$"))
                ModelState.AddModelError("PhoneNumber", "SĐT phải hợp lệ.");
            else if (_context.Users.Any(u => u.PhoneNumber == updatedUser.PhoneNumber && u.UserId != updatedUser.UserId))
                ModelState.AddModelError("PhoneNumber", "SĐT đã tồn tại.");

            if (string.IsNullOrWhiteSpace(updatedUser.Email))
                ModelState.AddModelError("Email", "Email không được để trống.");
            else if (!Regex.IsMatch(updatedUser.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") || updatedUser.Email.Length > 100)
                ModelState.AddModelError("Email", "Email không hợp lệ hoặc quá dài.");
            else if (_context.Users.Any(u => u.Email == updatedUser.Email && u.UserId != updatedUser.UserId))
                ModelState.AddModelError("Email", "Email đã tồn tại.");

            if (string.IsNullOrWhiteSpace(updatedUser.Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống.");

            if (!ModelState.IsValid)
                return View("Detail", updatedUser); // Trả về form Detail để hiển thị lỗi

            // So sánh từng trường để xác định có thay đổi gì không
            bool isChanged =
                user.FullName != updatedUser.FullName?.Trim() ||
                user.Cccd != updatedUser.Cccd?.Trim() ||
                user.PhoneNumber != updatedUser.PhoneNumber?.Trim() ||
                user.Address != updatedUser.Address?.Trim() ||
                user.Email != updatedUser.Email?.Trim() ||
                user.Role != updatedUser.Role ||
                user.IsActive != updatedUser.IsActive;

            if (!isChanged)
            {
                TempData["InfoMessage"] = "⚠️ Không có thay đổi nào được thực hiện.";
                return RedirectToAction("Detail", new { id = user.UserId });
            }

            // Nếu có thay đổi thì cập nhật
            user.FullName = updatedUser.FullName?.Trim();
            user.Cccd = updatedUser.Cccd?.Trim();
            user.PhoneNumber = updatedUser.PhoneNumber?.Trim();
            user.Address = updatedUser.Address?.Trim();
            user.Email = updatedUser.Email?.Trim();
            user.Role = updatedUser.Role;
            user.IsActive = updatedUser.IsActive;

            _context.Entry(user).State = EntityState.Modified;
            int count = await _context.SaveChangesAsync();

            Console.WriteLine("Records updated: " + count);
            TempData["SuccessMessage"] = "✅ Cập nhật người dùng thành công.";

            return RedirectToAction("Detail", new { id = user.UserId });
        }


    }
}
