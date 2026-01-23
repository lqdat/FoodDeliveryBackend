using System;

namespace FoodDeliveryBackend.API.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int Type { get; set; } // 1=Order, 2=Promo, 3=System
        public Guid? ReferenceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MarkReadRequest
    {
        public Guid? NotificationId { get; set; } // If null, mark all
    }
}
