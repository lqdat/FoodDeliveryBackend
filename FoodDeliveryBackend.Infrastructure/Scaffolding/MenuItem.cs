using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Infrastructure.Scaffolding;

public partial class MenuItem
{
    public Guid Id { get; set; }

    public Guid MenuCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameSecondary { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsPopular { get; set; }

    public int DisplayOrder { get; set; }

    public string? Options { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual MenuCategory MenuCategory { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
