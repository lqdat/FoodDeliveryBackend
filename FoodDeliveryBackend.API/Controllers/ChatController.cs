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
    public class ChatController : ControllerBase
    {
        private readonly FoodDeliveryDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(FoodDeliveryDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(Guid orderId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = int.Parse(User.FindFirstValue(ClaimTypes.Role)!);
            var isCustomer = role == 2;

            var query = _context.ChatMessages
                .Where(m => m.OrderId == orderId && !m.IsDeleted);
            
            // Filter by per-side soft delete
            if (isCustomer)
            {
                query = query.Where(m => !m.DeletedByCustomer);
            }
            else // Driver
            {
                query = query.Where(m => !m.DeletedByDriver);
            }

            var messages = await query
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    OrderId = m.OrderId,
                    SenderId = m.SenderId,
                    IsFromCustomer = m.IsFromCustomer,
                    Content = m.Content,
                    ImageUrl = m.ImageUrl,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = int.Parse(User.FindFirstValue(ClaimTypes.Role)!);
            var isCustomer = role == 2; // 2 = Customer

            var msg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                SenderId = userId,
                IsFromCustomer = isCustomer,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            var dto = new ChatMessageDto
            {
                Id = msg.Id,
                OrderId = msg.OrderId,
                SenderId = msg.SenderId,
                IsFromCustomer = msg.IsFromCustomer,
                Content = msg.Content,
                ImageUrl = msg.ImageUrl,
                IsRead = msg.IsRead,
                CreatedAt = msg.CreatedAt
            };

            // Push to Group via SignalR
            await _hubContext.Clients.Group($"Order-{request.OrderId}").SendAsync("ReceiveMessage", dto);

            return Ok(dto);
        }

        /// <summary>
        /// Mark a specific message as read
        /// </summary>
        [HttpPut("{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(Guid messageId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);
            
            if (message == null) return NotFound("Message not found");
            
            // Only the receiver can mark as read (not the sender)
            if (message.SenderId == userId) 
                return BadRequest("Cannot mark your own message as read");
            
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            // Notify sender that message was read
            await _hubContext.Clients.Group($"Order-{message.OrderId}").SendAsync("MessageRead", new { messageId, readAt = message.ReadAt });
            
            return Ok(new { success = true, messageId, readAt = message.ReadAt });
        }

        /// <summary>
        /// Mark all messages in an order as read (for current user)
        /// </summary>
        [HttpPut("{orderId}/read-all")]
        public async Task<IActionResult> MarkAllMessagesAsRead(Guid orderId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Get all unread messages that are NOT sent by current user
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.OrderId == orderId && !m.IsDeleted && !m.IsRead && m.SenderId != userId)
                .ToListAsync();
            
            if (!unreadMessages.Any()) 
                return Ok(new { success = true, count = 0, message = "No unread messages" });
            
            var now = DateTime.UtcNow;
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = now;
            }
            
            await _context.SaveChangesAsync();
            
            // Notify via SignalR
            await _hubContext.Clients.Group($"Order-{orderId}").SendAsync("AllMessagesRead", new { orderId, readAt = now, count = unreadMessages.Count });
            
            return Ok(new { success = true, count = unreadMessages.Count, readAt = now });
        }

        /// <summary>
        /// Get unread message count for an order
        /// </summary>
        [HttpGet("{orderId}/unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount(Guid orderId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Count unread messages that are NOT sent by current user
            var count = await _context.ChatMessages
                .CountAsync(m => m.OrderId == orderId && !m.IsDeleted && !m.IsRead && m.SenderId != userId);
            
            return Ok(new { orderId, unreadCount = count });
        }

        /// <summary>
        /// Get total unread message count across all orders for current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetTotalUnreadCount()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = int.Parse(User.FindFirstValue(ClaimTypes.Role)!);
            
            int count;
            if (role == 2) // Customer
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null) return Ok(new { unreadCount = 0 });
                
                // Get all orders for this customer
                var orderIds = await _context.Orders
                    .Where(o => o.CustomerId == customer.Id && !o.IsDeleted)
                    .Select(o => o.Id)
                    .ToListAsync();
                
                count = await _context.ChatMessages
                    .CountAsync(m => orderIds.Contains(m.OrderId) && !m.IsDeleted && !m.IsRead && m.SenderId != userId);
            }
            else if (role == 4) // Driver
            {
                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
                if (driver == null) return Ok(new { unreadCount = 0 });
                
                // Get all orders assigned to this driver
                var orderIds = await _context.Orders
                    .Where(o => o.DriverId == driver.Id && !o.IsDeleted)
                    .Select(o => o.Id)
                    .ToListAsync();
                
                count = await _context.ChatMessages
                    .CountAsync(m => orderIds.Contains(m.OrderId) && !m.IsDeleted && !m.IsRead && m.SenderId != userId);
            }
            else
            {
                count = 0;
            }
            
            return Ok(new { unreadCount = count });
        }

        /// <summary>
        /// Soft delete a message for current user's side only
        /// Customer deleting won't affect driver's view and vice versa
        /// </summary>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = int.Parse(User.FindFirstValue(ClaimTypes.Role)!);
            var isCustomer = role == 2;

            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

            if (message == null) return NotFound("Message not found");

            if (isCustomer)
            {
                message.DeletedByCustomer = true;
            }
            else // Driver
            {
                message.DeletedByDriver = true;
            }

            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, messageId, deletedBy = isCustomer ? "customer" : "driver" });
        }

        /// <summary>
        /// Soft delete all messages in an order for current user's side only
        /// </summary>
        [HttpDelete("{orderId}/all")]
        public async Task<IActionResult> DeleteAllMessages(Guid orderId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = int.Parse(User.FindFirstValue(ClaimTypes.Role)!);
            var isCustomer = role == 2;

            var messages = await _context.ChatMessages
                .Where(m => m.OrderId == orderId && !m.IsDeleted)
                .ToListAsync();

            if (!messages.Any()) 
                return Ok(new { success = true, count = 0, message = "No messages to delete" });

            var now = DateTime.UtcNow;
            foreach (var msg in messages)
            {
                if (isCustomer)
                {
                    msg.DeletedByCustomer = true;
                }
                else
                {
                    msg.DeletedByDriver = true;
                }
                msg.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, count = messages.Count, deletedBy = isCustomer ? "customer" : "driver" });
        }
    }
}
