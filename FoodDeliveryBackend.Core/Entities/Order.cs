using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public Guid RestaurantId { get; set; }

    public Guid? DriverId { get; set; }

    public Guid? VoucherId { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public double DeliveryLatitude { get; set; }

    public double DeliveryLongitude { get; set; }

    public string? DeliveryNote { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public int Status { get; set; }

    public int PaymentMethod { get; set; }

    public int PaymentStatus { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? PreparedAt { get; set; }

    public DateTime? PickedUpAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public int EstimatedDeliveryMinutes { get; set; }

    public double Distance { get; set; } // Distance in kilometers from Restaurant to Delivery

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual Driver? Driver { get; set; }

    public virtual ICollection<DriverEarning> DriverEarnings { get; set; } = new List<DriverEarning>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();

    public virtual Restaurant Restaurant { get; set; } = null!;

    public virtual Review? Review { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
