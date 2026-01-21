using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Review
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid CustomerId { get; set; }

    public int DriverRating { get; set; }

    public string? DriverComment { get; set; }

    public string? DriverTags { get; set; }

    public int FoodRating { get; set; }

    public string? FoodComment { get; set; }

    public string? ImageUrls { get; set; }

    public string? VideoUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
