using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class SearchHistory
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Keyword { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User User { get; set; } = null!;
}
