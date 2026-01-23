using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;
    private readonly IWebHostEnvironment _env;

    public TestController(FoodDeliveryDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetInfo()
    {
        var count = await _context.Restaurants.CountAsync();
        var restaurants = await _context.Restaurants
            .Include(r => r.MenuCategories)
            .ThenInclude(mc => mc.MenuItems)
            .ToListAsync();

        return Ok(new 
        { 
            Environment = _env.EnvironmentName,
            RestaurantCount = count,
            Restaurants = restaurants.Select(r => new 
            { 
                r.Id,
                r.MerchantId,
                r.Name, 
                CategoryCount = r.MenuCategories.Count,
                TotalItems = r.MenuCategories.Sum(mc => mc.MenuItems.Count)
            })
        });
    }
}
