using FoodDeliveryBackend.API.DTOs.ChainOwner;
using FoodDeliveryBackend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers.ChainOwner;

/// <summary>
/// Controller for Food item management.
/// Accessible by ChainOwner and StoreManager.
/// </summary>
[ApiController]
[Route("api/foods")]
[Authorize]
public class FoodController : ControllerBase
{
    private readonly IFoodService _foodService;

    public FoodController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    /// <summary>
    /// Create a new food item.
    /// Food is auto-submitted for approval.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FoodDto>> CreateFood([FromBody] CreateFoodDto dto)
    {
        try
        {
            var creatorId = GetCurrentAccountId();
            var accountType = User.FindFirst("AccountType")?.Value ?? "Unknown";

            var food = await _foodService.CreateFoodAsync(
                dto.StoreAccountId,
                creatorId,
                accountType,
                dto.Name,
                dto.Description,
                dto.Price,
                dto.OriginalPrice,
                dto.ImageUrl,
                dto.Category,
                dto.DisplayOrder);

            var response = new FoodDto(
                food.Id,
                food.StoreAccountId,
                food.Store?.StoreName ?? "",
                food.Name,
                food.Description,
                food.Price,
                food.OriginalPrice,
                food.ImageUrl,
                food.Category,
                food.Status.ToString(),
                food.IsAvailable,
                food.DisplayOrder,
                food.CreatedAt,
                food.UpdatedAt);

            return CreatedAtAction(nameof(GetFoodById), new { id = food.Id }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get food item by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FoodDto>> GetFoodById(Guid id)
    {
        var food = await _foodService.GetFoodByIdAsync(id);
        if (food == null)
        {
            return NotFound(new { message = "Food not found" });
        }

        return Ok(new FoodDto(
            food.Id,
            food.StoreAccountId,
            food.Store?.StoreName ?? "",
            food.Name,
            food.Description,
            food.Price,
            food.OriginalPrice,
            food.ImageUrl,
            food.Category,
            food.Status.ToString(),
            food.IsAvailable,
            food.DisplayOrder,
            food.CreatedAt,
            food.UpdatedAt));
    }

    /// <summary>
    /// Get all food items for a store.
    /// </summary>
    [HttpGet("store/{storeAccountId}")]
    public async Task<ActionResult<List<FoodDto>>> GetFoodsByStore(Guid storeAccountId)
    {
        var foods = await _foodService.GetFoodsByStoreAsync(storeAccountId);

        var dtos = foods.Select(f => new FoodDto(
            f.Id,
            f.StoreAccountId,
            f.Store?.StoreName ?? "",
            f.Name,
            f.Description,
            f.Price,
            f.OriginalPrice,
            f.ImageUrl,
            f.Category,
            f.Status.ToString(),
            f.IsAvailable,
            f.DisplayOrder,
            f.CreatedAt,
            f.UpdatedAt)).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Update a food item.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FoodDto>> UpdateFood(Guid id, [FromBody] UpdateFoodDto dto)
    {
        try
        {
            var updaterId = GetCurrentAccountId();
            var food = await _foodService.UpdateFoodAsync(
                id,
                updaterId,
                dto.Name,
                dto.Description,
                dto.Price,
                dto.OriginalPrice,
                dto.ImageUrl,
                dto.Category,
                dto.DisplayOrder,
                dto.IsAvailable);

            return Ok(new FoodDto(
                food.Id,
                food.StoreAccountId,
                food.Store?.StoreName ?? "",
                food.Name,
                food.Description,
                food.Price,
                food.OriginalPrice,
                food.ImageUrl,
                food.Category,
                food.Status.ToString(),
                food.IsAvailable,
                food.DisplayOrder,
                food.CreatedAt,
                food.UpdatedAt));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a food item.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFood(Guid id)
    {
        try
        {
            var deleterId = GetCurrentAccountId();
            await _foodService.DeleteFoodAsync(id, deleterId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid GetCurrentAccountId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(idClaim!);
    }
}
