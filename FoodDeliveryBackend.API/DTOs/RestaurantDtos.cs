namespace FoodDeliveryBackend.API.DTOs;

public class RestaurantDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string CoverUrl { get; set; } = null!; // For headers
    public string Address { get; set; } = null!;
    public double Rating { get; set; }
    public double Distance { get; set; }
    public string DeliveryTime { get; set; } = null!; // e.g. "15-20p"
    public string[] Tags { get; set; } = Array.Empty<string>();
    public bool IsFavorite { get; set; } 

    // Menu Sections
    public List<MenuSectionDto> MenuSections { get; set; } = new();
}

public class MenuSectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!; // "Món Nổi Bật", "Món Chính"
    public List<MenuItemDetailDto> Items { get; set; } = new();
}

public class MenuItemDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; } // For strike-through
    public string ImageUrl { get; set; } = null!;
    public bool IsPopular { get; set; } // "BÁN CHẠY" badge
    public string? DiscountBadge { get; set; } // "-15%" badge
}
