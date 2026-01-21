using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Restaurant
{
    public Guid Id { get; set; }

    public Guid MerchantId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameSecondary { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public string Address { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? PhoneNumber { get; set; }
    
    // Changing from string Category to Foreign Key
    public Guid? CategoryId { get; set; }
    public virtual FoodCategory? Category { get; set; }

    // Legacy support if needed, or we can drop this column in migration. 
    // Let's keep it for safety but mapped to 'CategoryName' maybe? 
    // Or just let EF handle it. If I remove it from C# model, EF will try to drop it.
    // I'll leave the property 'CategoryName' mapped to old 'Category' column if I wanted to preserve, 
    // but better to move forward. I'll NOT include 'public string Category' property.

    public bool IsOpen { get; set; }

    public TimeSpan? OpenTime { get; set; }

    public TimeSpan? CloseTime { get; set; }

    // Frontend: deliveryTime (int, minutes)
    public int DeliveryTime { get; set; }

    public decimal DeliveryFee { get; set; }

    // Frontend: minPrice (decimal)
    public decimal MinPrice { get; set; }

    // Frontend: rating (double)
    public double Rating { get; set; }

    // Frontend: ratingCount (int) - Renaming TotalReviews to RatingCount
    public int RatingCount { get; set; }

    public bool HasPromotion { get; set; }

    public bool IsNew { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsApproved { get; set; }

    public bool IsTrending { get; set; }

    public int TotalOrders { get; set; }
    
    // New Fields
    public double Distance { get; set; }
    
    // Storing Tags as comma separated string or JSON?
    // Frontend expects string[] tags.
    // For simplicity in EF Core Postgres, we can stick to string and split, or use string[].
    // Npgsql supports string[] natively.
    public string[]? Tags { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
