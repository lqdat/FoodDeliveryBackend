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
public class CategoriesController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public CategoriesController(FoodDeliveryDbContext context)
    {
        _context = context;
    }



    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoodCategory>>> GetCategories()
    {
        return await _context.FoodCategories
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    [HttpGet("code/{code}/restaurants")]
    public async Task<ActionResult<CategoryWithRestaurantsDto>> GetCategoryWithRestaurants(string code)
    {
        var category = await _context.FoodCategories
            .Where(c => c.Code.ToUpper() == code.ToUpper() && !c.IsDeleted && c.IsActive)
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound(new { message = $"Category with code '{code}' not found." });
        }

        // Fetch restaurants for this category
        var restaurants = await _context.Restaurants
            .Where(r => r.CategoryId == category.Id && !r.IsDeleted && r.IsApproved)
            .Select(r => new RestaurantSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                ImageUrl = r.ImageUrl,
                CoverUrl = r.CoverImageUrl ?? r.ImageUrl,
                Rating = r.Rating,
                Distance = r.Distance,
                DeliveryTime = r.DeliveryTime,
                MinPrice = r.MinPrice,
                IsTrending = r.IsTrending
            })
            .ToListAsync();

        return new CategoryWithRestaurantsDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Restaurants = restaurants
        };
    }
}
