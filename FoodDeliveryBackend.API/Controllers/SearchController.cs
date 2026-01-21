using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public SearchController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResponseDto>> Search(
        [FromQuery] string? q, 
        [FromQuery] string? sortBy, // "rating", "distance", "price_low"
        [FromQuery] bool isFreeship = false)
    {
        // 1. Base Query for Restaurants
        var restQuery = _context.Restaurants
            .Where(r => !r.IsDeleted && r.IsApproved && r.IsOpen);

        // Filter by Keyword
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.ToLower().Trim();
            restQuery = restQuery.Where(r => r.Name.ToLower().Contains(q));
        }

        // Filter by Freeship
        if (isFreeship)
        {
            restQuery = restQuery.Where(r => r.DeliveryFee == 0);
        }

        // Apply Sorting
        switch (sortBy?.ToLower())
        {
            case "rating":
                restQuery = restQuery.OrderByDescending(r => r.Rating);
                break;
            case "distance": // "Gần tôi"
                restQuery = restQuery.OrderBy(r => r.Distance);
                break;
            case "price_low":
                restQuery = restQuery.OrderBy(r => r.MinPrice); // Assuming MinPrice represents base price level
                break;
            default:
                // Default sort, maybe by relevance or ID?
                // If checking "Gần tôi" without keyword, effectively it is distance sort.
                if (string.IsNullOrWhiteSpace(q)) restQuery = restQuery.OrderBy(r => r.Distance); 
                break;
        }

        var restaurants = await restQuery
            .Take(20)
            .Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                ImageUrl = r.ImageUrl,
                Address = r.Address,
                Rating = r.Rating,
                Distance = r.Distance,
                DeliveryTime = r.DeliveryTime,
                Tags = r.Tags
            })
            .ToListAsync();

        // 2. Search Menu Items (Foods) - Only if keyword provided
        // Food filtering by 'isFreeship' generally means 'Restaurant has Freeship'
        List<MenuItemDto> foods = new();
        if (!string.IsNullOrWhiteSpace(q))
        {
             var foodQuery = _context.MenuItems
                .Include(m => m.MenuCategory)
                    .ThenInclude(mc => mc.Restaurant)
                .Where(m => !m.IsDeleted && m.IsAvailable && 
                           (m.Name.ToLower().Contains(q) || (m.Description != null && m.Description.ToLower().Contains(q))));
             
             if (isFreeship)
             {
                 foodQuery = foodQuery.Where(m => m.MenuCategory.Restaurant.DeliveryFee == 0);
             }

             // Sort Foods
             switch (sortBy?.ToLower())
             {
                case "price_low":
                    foodQuery = foodQuery.OrderBy(m => m.Price);
                    break;
                 // Other sorts like 'rating' apply to Restaurant usually.
             }

             foods = await foodQuery
                .Take(20)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl,
                    RestaurantId = m.MenuCategory.RestaurantId,
                    RestaurantName = m.MenuCategory.Restaurant.Name
                })
                .ToListAsync();
        }

        return new SearchResponseDto
        {
            Restaurants = restaurants,
            Foods = foods
        };
    }
}
