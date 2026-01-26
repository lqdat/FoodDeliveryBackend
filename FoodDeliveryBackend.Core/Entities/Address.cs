using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Address
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string Label { get; set; } = null!; // Classification: Home/Work

    public string? Name { get; set; } // Memorable Name: "Nha vo", "Nha nguoi yeu"

    public string FullAddress { get; set; } = null!;

    public string? Note { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastUsedAt { get; set; } // Track when address was last used for an order

    public bool IsDeleted { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
