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
    public async Task<ActionResult<RestaurantDetailDto>> GetRestaurant(
        Guid id, 
        [FromQuery] string? section, // e.g. "Món Chính"
        [FromQuery] bool? isPopular // e.g. true
    )
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuCategories)
                .ThenInclude(mc => mc.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound();
        }

        // Fetch Reviews for this Restaurant (via Orders)
        var reviews = await _context.Reviews
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Where(r => r.Order.RestaurantId == id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20) // Limit to 20 recent reviews
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                CustomerName = r.Customer.User.FullName,
                Rating = r.FoodRating, // Assuming FoodRating represents the user's satisfaction with the meal/restaurant
                Comment = r.FoodComment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        // Map to DTO
        var sections = new List<MenuSectionDto>();

        // 1. "Dành cho bạn" / "Món Nổi Bật" Section (Popular items)
        // If filtering by specific section other than "Món Nổi Bật", skip this unless requested
        bool showPopular = (string.IsNullOrEmpty(section) || section.Equals("Món Nổi Bật", StringComparison.OrdinalIgnoreCase)) 
                           && (isPopular == null || isPopular == true);

        if (showPopular)
        {
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
                    Name = "Món Nổi Bật",
                    Items = popularItems
                });
            }
        }

        // 2. Regular Categories
        // Default: Show all if no filter
        var categories = restaurant.MenuCategories.AsQueryable();

        if (!string.IsNullOrEmpty(section) && !section.Equals("Món Nổi Bật", StringComparison.OrdinalIgnoreCase))
        {
            categories = categories.Where(c => c.Name.ToLower() == section.ToLower());
        }

        foreach (var category in categories.OrderBy(c => c.DisplayOrder))
        {
             // If filtering by popular items only within categories is weird, usually 'isPopular' means just the "Featured" section.
             // But let's respect the flag if passed explicitly.
             var itemsQuery = category.MenuItems
                .Where(m => !m.IsDeleted && m.IsAvailable);

             if (isPopular == true) 
             {
                 itemsQuery = itemsQuery.Where(m => m.IsPopular);
             }

            var items = itemsQuery
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
            Latitude = restaurant.Latitude,
            Longitude = restaurant.Longitude,
            DeliveryTime = $"{restaurant.DeliveryTime} min", 
            Tags = restaurant.Tags ?? new[] { "Món ngon", "Gần bạn" },
            IsFavorite = false, // TODO: Check with user favorites
            RatingCount = restaurant.RatingCount,
            Reviews = reviews,
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
