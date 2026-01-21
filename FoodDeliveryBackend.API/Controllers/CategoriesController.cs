using FoodDeliveryBackend.Core.Entities;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Controllers;

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

    [HttpGet("{id}")]
    public async Task<ActionResult<FoodCategory>> GetCategory(Guid id)
    {
        var category = await _context.FoodCategories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.IsActive);
        
        if (category == null)
        {
            return NotFound();
        }
        
        return category;
    }
}
