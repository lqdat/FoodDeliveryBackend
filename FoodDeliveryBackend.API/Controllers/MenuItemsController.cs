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
            .Include(m => m.OrderItems) // To potentiall calc popularity or rating?
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        // Real Rating Calculation
        // 1. Find all completed orders containing this item that have reviews
        var reviews = await _context.Reviews
            .Include(r => r.Customer)
            .Where(r => r.Order.OrderItems.Any(oi => oi.MenuItemId == id) && !r.IsDeleted)
            .ToListAsync();

        double rating = 0;
        int ratingCount = reviews.Count;

        if (ratingCount > 0)
        {
            rating = reviews.Average(r => r.FoodRating);
        }

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
            Rating = Math.Round(rating, 1),
            RatingCount = ratingCount,
            Size = "Tiêu chuẩn",
            Calories = 300 + new Random(id.GetHashCode()).Next(200), // Mock calories allowed
            Options = options
        };
    }

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<object>> GetMenuItemReviews(Guid id)
    {
         var reviews = await _context.Reviews
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Where(r => r.Order.OrderItems.Any(oi => oi.MenuItemId == id) && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new 
            {
                r.Id,
                CustomerName = r.Customer.User.FullName, // Fixed access
                r.FoodRating,
                r.FoodComment,
                r.CreatedAt
            })
            .ToListAsync();
            
        return Ok(reviews);
    }
}
