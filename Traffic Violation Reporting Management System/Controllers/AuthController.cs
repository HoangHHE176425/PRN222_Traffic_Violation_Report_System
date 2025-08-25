using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Traffic_Violation_Reporting_Management_System.DTOs;
using Traffic_Violation_Reporting_Management_System.Service;

namespace Traffic_Violation_Reporting_Management_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

       
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                
                if (!await _authService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống");
                    return View(model);
                }

                
                if (!await _authService.IsUserActiveAsync(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị vô hiệu hóa");
                    return View(model);
                }

                
                var user = await _authService.ValidateUserAsync(model.Email, model.Password);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
                    return View(model);
                }

                
                var claims = _authService.CreateUserClaims(user);
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe 
                        ? DateTimeOffset.UtcNow.AddDays(30) 
                        : DateTimeOffset.UtcNow.AddHours(24)
                };

               
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetInt32("Role", user.Role);
                HttpContext.Session.SetString("Email", user.Email);


                _logger.LogInformation("User {Email} đã đăng nhập thành công lúc {Time}", 
                    user.Email, DateTime.Now);

                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng nhập cho email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại.");
                return View(model);
            }
        }

        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                _logger.LogInformation("User {Email} đã đăng xuất lúc {Time}", 
                    userEmail, DateTime.Now);

                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng xuất");
                return RedirectToAction("Login", "Auth");
            }
        }

        
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new ForgotPasswordRequest());
        }

        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.ForgotPasswordAsync(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    TempData["Email"] = model.Email;
                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình xử lý forgot password cho email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại.");
                return View(model);
            }
        }

        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordRequest();
            
            if (!string.IsNullOrEmpty(email))
            {
                model.Email = email;
            }
            else if (TempData["Email"] != null)
            {
                model.Email = TempData["Email"].ToString() ?? "";
                TempData.Keep("Email"); 
            }

            return View(model);
        }

        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.ResetPasswordAsync(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình reset password cho email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đặt lại mật khẩu. Vui lòng thử lại.");
                return View(model);
            }
        }

       
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterRequest());
        }

        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.RegisterUserAsync(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    TempData["Email"] = model.Email;
                    return RedirectToAction("VerifyOtp");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng ký cho email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
                return View(model);
            }
        }

       
        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp(string? email = null)
        {
            
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new VerifyOtpRequest();
            
            
            if (!string.IsNullOrEmpty(email))
            {
                model.Email = email;
            }
            else if (TempData["Email"] != null)
            {
                model.Email = TempData["Email"].ToString() ?? "";
                TempData.Keep("Email"); 
            }

            return View(model);
        }

    
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.VerifyOtpAsync(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình xác thực OTP cho email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình xác thực. Vui lòng thử lại.");
                return View(model);
            }
        }



    }
}
