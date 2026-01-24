using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class FoodCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!; // Unique code for category

    public string? NameSecondary { get; set; }

    public string? IconUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? BackgroundColor { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
