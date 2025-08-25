using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Traffic_Violation_Reporting_Management_System;
using Traffic_Violation_Reporting_Management_System.Models;
using Traffic_Violation_Reporting_Management_System.Service;
using Traffic_Violation_Reporting_Management_System.Service;

var builder = WebApplication.CreateBuilder(args);

// === DB ===
builder.Services.AddDbContext<TrafficViolationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === Email / Config ===
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<PayOsConfig>(builder.Configuration.GetSection("PayOS"));

// === Auth (Cookie) ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// === MVC + JSON ===
builder.Services.AddControllersWithViews().AddNewtonsoftJson();

// === Session ===
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// === SignalR (Realtime Notifications) ===
builder.Services.AddSignalR(); // <— quan tr?ng

// === DI Services ===
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<SmsService>();
builder.Services.AddHttpClient<PayOSService>();

// Thêm NotificationService
builder.Services.AddScoped<INotificationService, NotificationService>(); // <— quan tr?ng

// (N?u front-end khác domain, m? CORS bên d??i)
// builder.Services.AddCors(o => o.AddPolicy("AllowAll",
//     p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
//           .SetIsOriginAllowed(_ => true)));

builder.Logging.AddConsole();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // ?ang t?t theo comment c?a b?n
app.UseStaticFiles();

app.UseRouting();

// app.UseCors("AllowAll"); // b?t n?u dùng CORS ? trên

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// === Endpoints ===
app.MapControllers();

// Map hub cho nút chuông notification
app.MapHub<NotificationHub>("/hubs/notifications"); // <— quan tr?ng

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
