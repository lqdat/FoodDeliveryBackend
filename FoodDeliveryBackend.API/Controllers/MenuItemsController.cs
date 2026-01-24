using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FoodDeliveryBackend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/menu-items")]
public class MenuItemsController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public MenuItemsController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MenuItemFullDetailDto>> GetMenuItem(Guid id)
    {
        var item = await _context.MenuItems
            .Include(m => m.MenuCategory)
                .ThenInclude(mc => mc.Restaurant) // Include Restaurant to get Rating
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        // User requirement: "Rating is Restaurant Rating"
        double rating = item.MenuCategory.Restaurant.Rating;
        int ratingCount = item.MenuCategory.Restaurant.RatingCount; 

        // Parse Options (Assuming stored as JSON string)
        object? options = null;
        if (!string.IsNullOrEmpty(item.Options))
        {
            try { options = JsonSerializer.Deserialize<object>(item.Options); } catch { }
        }

        string? badge = null;
        if (item.OriginalPrice > item.Price)
        {
            int percent = (int)((1 - item.Price / item.OriginalPrice) * 100);
            badge = $"-{percent}%";
        }

        return new MenuItemFullDetailDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description ?? "Món ngon chất lượng.",
            Price = item.Price,
            OriginalPrice = item.OriginalPrice,
            ImageUrl = item.ImageUrl ?? "https://placeholder.com/food",
            IsPopular = item.IsPopular,
            DiscountBadge = badge,
            Rating = rating,
            RatingCount = ratingCount,
            Size = "Tiêu chuẩn",
            Calories = 300 + new Random(id.GetHashCode()).Next(200), // Mock calories allowed
            Options = options
        };
    }
}
