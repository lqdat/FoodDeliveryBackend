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
            // Ideally check if user belongs to order (Customer or Driver)

            var messages = await _context.ChatMessages
                .Where(m => m.OrderId == orderId && !m.IsDeleted)
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
    }
}
