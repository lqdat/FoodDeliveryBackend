using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class MenuCategory
{
    public Guid Id { get; set; }

    public Guid RestaurantId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameSecondary { get; set; }

    public string? Description { get; set; }

    public string? IconName { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual Restaurant Restaurant { get; set; } = null!;
}
