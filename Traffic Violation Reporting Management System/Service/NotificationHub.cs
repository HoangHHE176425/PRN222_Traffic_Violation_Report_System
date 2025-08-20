using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Traffic_Violation_Reporting_Management_System.Service
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // Lấy userId từ claim NameIdentifier
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SignalR connected WITHOUT NameIdentifier. ConnectionId={ConnectionId}, User={User}",
                    Context.ConnectionId, Context.User?.Identity?.Name);
                // Không có UID ⇒ không join group được ⇒ hủy kết nối
                Context.Abort();
                return;
            }

            var group = $"user:{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            _logger.LogInformation("SignalR connected. ConnId={ConnectionId}, UserId={UserId}, Group={Group}",
                Context.ConnectionId, userId, group);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var group = $"user:{userId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

                _logger.LogInformation("SignalR disconnected. ConnId={ConnectionId}, UserId={UserId}, Group={Group}, Error={Error}",
                    Context.ConnectionId, userId, group, exception?.Message);
            }
            else
            {
                _logger.LogInformation("SignalR disconnected WITHOUT NameIdentifier. ConnId={ConnectionId}, Error={Error}",
                    Context.ConnectionId, exception?.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Optional: method test để kiểm tra kết nối từ client
        public Task<string> Ping() => Task.FromResult("pong");
    }
}
