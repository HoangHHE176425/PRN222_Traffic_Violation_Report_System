using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Traffic_Violation_Reporting_Management_System.Models;
using Traffic_Violation_Reporting_Management_System.DTOs;

namespace Traffic_Violation_Reporting_Management_System.Service
{
    public class AuthService
    {
        private readonly TrafficViolationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(TrafficViolationDbContext context, IEmailService emailService, ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive == true);

            if (user == null)
                return null;

           
            if (password == user.Password)
                return user;

            return null;
        }

        
        public List<Claim> CreateUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim("CCCD", user.Cccd),
                new Claim(ClaimTypes.Role, GetRoleName(user.Role))
            };

            return claims;
        }

       
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

       
        public async Task<bool> IsUserActiveAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            
            return user?.IsActive ?? false;
        }

        
        public string HashPassword(string password)
        {
            
            return password;
            
            /* Code hash SHA256 (nếu hash thì bỏ cmt nhé)
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
            */
        }

       
        public bool VerifyPassword(string password, string storedPassword)
        {
            
            return password == storedPassword;
            
            
        }

       
        public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Bắt đầu đăng ký user với email: {Email}", request.Email);
                
                if (await EmailExistsAsync(request.Email))
                {
                    return (false, "Email đã được đăng ký trong hệ thống");
                }

                
                if (await CccdExistsAsync(request.Cccd))
                {
                    return (false, "Số CCCD đã được đăng ký trong hệ thống");
                }

                
                if (await PhoneExistsAsync(request.PhoneNumber))
                {
                    return (false, "Số điện thoại đã được đăng ký trong hệ thống");
                }

                
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    
                    var newUser = new User
                    {
                        FullName = request.FullName.Trim(),
                        Cccd = request.Cccd.Trim(),
                        PhoneNumber = request.PhoneNumber.Trim(),
                        Email = request.Email.Trim().ToLower(),
                        Address = request.Address?.Trim(),
                        Password = request.Password, 
                        Role = 0,
                        IsActive = false, 
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User đã được tạo thành công với ID: {UserId}", newUser.UserId);

                    
                    var otpCode = GenerateOtpCode();
                    var otp = new Otp
                    {
                        Email = request.Email, 
                        PhoneNumber = "", 
                        Otpcode = otpCode,
                        CreatedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddMinutes(10), 
                        IsUsed = false
                    };

                    _context.Otps.Add(otp);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("OTP đã được tạo trong database: {OtpCode}", otpCode);

                    
                    _logger.LogInformation("Bắt đầu gửi email OTP tới {Email}", request.Email);
                    var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode, request.FullName);
                    
                    if (!emailSent)
                    {
                        _logger.LogError("❌ Gửi email OTP thất bại cho {Email}", request.Email);
                        throw new Exception("Không thể gửi email xác thực");
                    }

                    
                    await transaction.CommitAsync();
                    _logger.LogInformation("✅ Transaction commit thành công - User và OTP đã được tạo, email đã gửi");
                    
                    return (true, "Đăng ký thành công! Vui lòng kiểm tra email để nhận mã xác thực.");
                }
                catch (Exception transactionEx)
                {
                    
                    await transaction.RollbackAsync();
                    _logger.LogError(transactionEx, "❌ Transaction rollback - Lỗi trong quá trình tạo user hoặc gửi OTP: {Message}", transactionEx.Message);
                    return (false, $"Không thể hoàn thành đăng ký: {transactionEx.Message}. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng ký user với email: {Email}", request.Email);
                return (false, "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
            }
        }



      
        public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request)
        {
            try
            {
                
                var otp = await _context.Otps
                    .Where(o => o.Email == request.Email.ToLower() 
                               && o.Otpcode == request.OtpCode
                               && !o.IsUsed
                               && o.ExpiresAt > DateTime.Now)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    return (false, "Mã OTP không hợp lệ hoặc đã hết hạn");
                }

                
                otp.IsUsed = true;

              
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    return (false, "Không tìm thấy tài khoản");
                }

                user.IsActive = true;
                await _context.SaveChangesAsync();

                
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                return (true, "Xác thực thành công! Tài khoản đã được kích hoạt.");
            }
            catch (Exception ex)
            {
                return (false, "Có lỗi xảy ra trong quá trình xác thực");
            }
        }



        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Bắt đầu xử lý forgot password cho email: {Email}", request.Email);
               
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive == true);

                if (user == null)
                {
                    return (false, "Email không tồn tại hoặc tài khoản chưa được kích hoạt");
                }

                
                var oldOtps = await _context.Otps
                    .Where(o => o.Email == request.Email.ToLower())
                    .ToListAsync();
                
                if (oldOtps.Any())
                {
                    _logger.LogInformation("Xóa {Count} OTP cũ cho {Email}", oldOtps.Count, request.Email);
                    _context.Otps.RemoveRange(oldOtps);
                }

                
                var otpCode = GenerateOtpCode();
                _logger.LogInformation("Đã tạo OTP code cho forgot password: {OtpCode} cho {Email}", otpCode, request.Email);
                
                var otp = new Otp
                {
                    Email = request.Email.ToLower(),
                    PhoneNumber = "", 
                    Otpcode = otpCode,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(10), 
                    IsUsed = false
                };

                _context.Otps.Add(otp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã lưu OTP forgot password vào database cho {Email}", request.Email);

               
                var emailSent = await _emailService.SendForgotPasswordOtpEmailAsync(request.Email, otpCode, user.FullName);
                
                if (!emailSent)
                {
                    _logger.LogError("❌ Gửi email OTP forgot password thất bại cho {Email}", request.Email);
                    return (false, "Không thể gửi email xác thực");
                }

                _logger.LogInformation("✅ Gửi email OTP forgot password thành công cho {Email}", request.Email);
                return (true, "Mã xác thực đã được gửi qua email. Vui lòng kiểm tra hộp thư.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception khi xử lý forgot password cho {Email}: {Message}", request.Email, ex.Message);
                return (false, "Có lỗi xảy ra khi gửi mã xác thực. Vui lòng thử lại.");
            }
        }

        
        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Bắt đầu reset password cho email: {Email}", request.Email);
                
                
                var otp = await _context.Otps
                    .Where(o => o.Email == request.Email.ToLower() 
                               && o.Otpcode == request.OtpCode
                               && !o.IsUsed
                               && o.ExpiresAt > DateTime.Now)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    return (false, "Mã OTP không hợp lệ hoặc đã hết hạn");
                }

                
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive == true);

                if (user == null)
                {
                    return (false, "Không tìm thấy tài khoản");
                }

               
                otp.IsUsed = true;

                
                user.Password = request.NewPassword; 

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Reset password thành công cho {Email}", request.Email);

                return (true, "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception khi reset password cho {Email}: {Message}", request.Email, ex.Message);
                return (false, "Có lỗi xảy ra trong quá trình đặt lại mật khẩu. Vui lòng thử lại.");
            }
        }

        
        public async Task<bool> CccdExistsAsync(string cccd)
        {
            return await _context.Users
                .AnyAsync(u => u.Cccd == cccd);
        }

       
        public async Task<bool> PhoneExistsAsync(string phoneNumber)
        {
            return await _context.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

       
        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        /// <summary>
        /// Vì k có bảng role nên a tạo tạm 3 role này nhé
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                0 => "Admin",
                1 => "Officer", 
                2 => "Citizen", 
                _ => "Unknown"
            };
        }
    }
}
