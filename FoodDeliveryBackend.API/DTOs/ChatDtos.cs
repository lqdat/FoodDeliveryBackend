using System;

namespace FoodDeliveryBackend.API.DTOs
{
    public class SendMessageRequest
    {
        public Guid OrderId { get; set; }
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }

    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid SenderId { get; set; }
        public bool IsFromCustomer { get; set; }
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
