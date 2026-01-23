using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Core.Entities;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace FoodDeliveryBackend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public RestaurantsController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        return await _context.Restaurants
            .Where(r => !r.IsDeleted && r.IsApproved)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurantsByCategory(Guid categoryId)
    {
        // Step 2: Find restaurants matching by CategoryId
        return await _context.Restaurants
            .Where(r => !r.IsDeleted && r.IsApproved && r.CategoryId == categoryId)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDetailDto>> GetRestaurant(Guid id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuCategories)
                .ThenInclude(mc => mc.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound();
        }

        // Map to DTO
        var sections = new List<MenuSectionDto>();

        // 1. "Dành cho bạn" / "Món Nổi Bật" Section (Popular items)
        var popularItems = restaurant.MenuCategories
            .SelectMany(mc => mc.MenuItems)
            .Where(m => m.IsPopular && !m.IsDeleted && m.IsAvailable)
            .Take(5)
            .Select(MapToMenuItemDto)
            .ToList();

        if (popularItems.Any())
        {
            sections.Add(new MenuSectionDto
            {
                Id = Guid.NewGuid(),
                Name = "Món Nổi Bật", // Matches "Dành cho bạn" implicitly or explicit section
                Items = popularItems
            });
        }

        // 2. Regular Categories
        foreach (var category in restaurant.MenuCategories.OrderBy(c => c.DisplayOrder))
        {
            var items = category.MenuItems
                .Where(m => !m.IsDeleted && m.IsAvailable)
                .OrderBy(m => m.DisplayOrder)
                .Select(MapToMenuItemDto)
                .ToList();

            if (items.Any())
            {
                sections.Add(new MenuSectionDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Items = items
                });
            }
        }

        return new RestaurantDetailDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            ImageUrl = restaurant.ImageUrl,
            CoverUrl = restaurant.CoverImageUrl ?? restaurant.ImageUrl, // Fallback
            Address = restaurant.Address,
            Rating = restaurant.Rating,
            Distance = restaurant.Distance,
            DeliveryTime = $"{restaurant.DeliveryTime} min", 
            Tags = restaurant.Tags ?? new[] { "Món ngon", "Gần bạn" },
            IsFavorite = false, // TODO: Check with user favorites
            MenuSections = sections
        };
    }

    private MenuItemDetailDto MapToMenuItemDto(MenuItem m)
    {
        string? badge = null;
        if (m.OriginalPrice > m.Price)
        {
            int percent = (int)((1 - m.Price / m.OriginalPrice) * 100);
            badge = $"-{percent}%";
        }

        return new MenuItemDetailDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description ?? "",
            Price = m.Price,
            OriginalPrice = m.OriginalPrice,
            ImageUrl = m.ImageUrl ?? "https://placeholder.com/food", // Placeholder if null
            IsPopular = m.IsPopular,
            DiscountBadge = badge
        };
    }
}
