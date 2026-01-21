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

        // Filter by Keyword using Full-Text Search (Vietnamese)
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            // Using "simple" configuration for wide matching or "vietnamese" if installed. 
            // "simple" is safe default. ToTsVector generates vector on the fly.
            // WebSearchToTsQuery handles "foo bar" as "foo & bar".
            restQuery = restQuery.Where(r => 
                EF.Functions.ToTsVector("simple", r.Name + " " + (r.Address ?? "") + " " + string.Join(" ", r.Tags ?? Array.Empty<string>()))
                .Matches(EF.Functions.WebSearchToTsQuery("simple", q)));
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
                restQuery = restQuery.OrderBy(r => r.MinPrice);
                break;
            default:
                // If searching by keyword, sort by Rank (Relevance)
                if (!string.IsNullOrWhiteSpace(q))
                {
                    restQuery = restQuery.OrderByDescending(r => 
                        EF.Functions.ToTsVector("simple", r.Name + " " + (r.Address ?? "") + " " + string.Join(" ", r.Tags ?? Array.Empty<string>()))
                        .Rank(EF.Functions.WebSearchToTsQuery("simple", q)));
                } 
                else 
                {
                    restQuery = restQuery.OrderBy(r => r.Distance); 
                }
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

        // 2. Search Menu Items (Foods)
        List<MenuItemDto> foods = new();
        if (!string.IsNullOrWhiteSpace(q))
        {
             var foodQuery = _context.MenuItems
                .Include(m => m.MenuCategory)
                    .ThenInclude(mc => mc.Restaurant)
                .Where(m => !m.IsDeleted && m.IsAvailable);

             // Full Text Search on Food Name and Description
             foodQuery = foodQuery.Where(m => 
                EF.Functions.ToTsVector("simple", m.Name + " " + (m.Description ?? ""))
                .Matches(EF.Functions.WebSearchToTsQuery("simple", q)));

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
                default:
                    // Sort by Rank
                    foodQuery = foodQuery.OrderByDescending(m => 
                        EF.Functions.ToTsVector("simple", m.Name + " " + (m.Description ?? ""))
                        .Rank(EF.Functions.WebSearchToTsQuery("simple", q)));
                    break;
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
