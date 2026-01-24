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
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DeliveryTime { get; set; } = null!; // e.g. "15-20p"
    public string[] Tags { get; set; } = Array.Empty<string>();
    public bool IsFavorite { get; set; } 
    public int RatingCount { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();

    // Menu Sections
    public List<MenuSectionDto> MenuSections { get; set; } = new();
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
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

public class MenuItemFullDetailDto : MenuItemDetailDto
{
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    public string Size { get; set; } = "Vừa"; // Default size description
    public int Calories { get; set; } // e.g. 500 kcal
    public object? Options { get; set; } // JSON Parsed options
}

public class RestaurantSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string CoverUrl { get; set; } = null!;
    public double Rating { get; set; }
    public double Distance { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DeliveryTime { get; set; }
    public decimal MinPrice { get; set; }
    public bool IsTrending { get; set; }
}

public class CategoryWithRestaurantsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public List<RestaurantSummaryDto> Restaurants { get; set; } = new();
}
