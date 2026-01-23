using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.API.Hubs;
using FoodDeliveryBackend.Core.Entities;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly FoodDeliveryDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(FoodDeliveryDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = Guid.Parse(userIdStr);

            var notifs = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    ReferenceId = n.ReferenceId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifs);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var count = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
            return Ok(new { count });
        }

        [HttpPut("read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkReadRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (request.NotificationId.HasValue)
            {
                var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId);
                if (notif != null)
                {
                    notif.IsRead = true;
                    notif.ReadAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Mark all
                var notifs = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
                foreach (var n in notifs)
                {
                    n.IsRead = true;
                    n.ReadAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Internal endpoint to create notification (for testing or internal calls)
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto dto)
        {
             // For testing, assuming we send to current user or a specific user if provided in a wrapper (omitted for brevity, using current User for test)
             var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
             
             var notif = new Notification
             {
                 Id = Guid.NewGuid(),
                 UserId = userId, // Self-send for test
                 Title = dto.Title,
                 Message = dto.Message,
                 Type = dto.Type,
                 ReferenceId = dto.ReferenceId,
                 CreatedAt = DateTime.UtcNow,
                 IsRead = false
             };
             
             _context.Notifications.Add(notif);
             await _context.SaveChangesAsync();
             
             dto.Id = notif.Id;
             dto.CreatedAt = notif.CreatedAt;

             // Push via SignalR
             await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", dto);
             
             return Ok(dto);
        }
    }
}
