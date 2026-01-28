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

    public class NotificationDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int Type { get; set; }
        public string TypeName { get; set; } = null!; // "Đơn hàng", "Khuyến mãi", "Hệ thống"
        public Guid? ReferenceId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request for batch deleting notifications
    /// </summary>
    public class DeleteNotificationsBatchRequest
    {
        /// <summary>
        /// List of notification IDs to delete
        /// </summary>
        public List<Guid> Ids { get; set; } = new();
    }
}
