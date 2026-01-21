using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class ChatMessage
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid SenderId { get; set; }

    public bool IsFromCustomer { get; set; }

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;
}
