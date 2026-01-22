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
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
    {
        return await _context.Restaurants
            .Where(r => !r.IsDeleted && r.IsApproved)
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
    public async Task<ActionResult<Restaurant>> GetRestaurant(Guid id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuCategories)
            .ThenInclude(mc => mc.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound();
        }

        return restaurant;
    }
}
