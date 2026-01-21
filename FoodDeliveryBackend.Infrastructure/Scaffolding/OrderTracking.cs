using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Infrastructure.Scaffolding;

public partial class OrderTracking
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public int Status { get; set; }

    public string? Description { get; set; }

    public string? DescriptionSecondary { get; set; }

    public DateTime Timestamp { get; set; }

    public double? DriverLatitude { get; set; }

    public double? DriverLongitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;
}
