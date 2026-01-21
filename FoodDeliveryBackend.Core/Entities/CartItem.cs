using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class CartItem
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }

    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? Notes { get; set; }

    public string? SelectedOptions { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual MenuItem MenuItem { get; set; } = null!;
}
