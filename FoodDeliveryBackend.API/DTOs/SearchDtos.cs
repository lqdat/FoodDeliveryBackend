using FoodDeliveryBackend.Core.Entities;

namespace FoodDeliveryBackend.API.DTOs;

public class SearchResponseDto
{
    public List<RestaurantDto> Restaurants { get; set; } = new();
    public List<MenuItemDto> Foods { get; set; } = new();
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = null!;
}

public class RestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Address { get; set; } = null!;
    public double Rating { get; set; }
    public double? Distance { get; set; }
    public int DeliveryTime { get; set; }
    public string[]? Tags { get; set; }
}
