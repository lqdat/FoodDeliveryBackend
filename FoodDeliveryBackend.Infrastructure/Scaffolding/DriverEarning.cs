using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Infrastructure.Scaffolding;

public partial class DriverEarning
{
    public Guid Id { get; set; }

    public Guid DriverId { get; set; }

    public Guid? OrderId { get; set; }

    public int Type { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime EarnedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual Order? Order { get; set; }
}
