using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Infrastructure.Scaffolding;

public partial class Voucher
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Type { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MaxDiscountAmount { get; set; }

    public decimal MinOrderAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? MaxUsage { get; set; }

    public int UsedCount { get; set; }

    public bool IsActive { get; set; }

    public string? Category { get; set; }

    public string? IconUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
