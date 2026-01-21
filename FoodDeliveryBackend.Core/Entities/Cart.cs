using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Cart
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid RestaurantId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual Restaurant Restaurant { get; set; } = null!;
}
