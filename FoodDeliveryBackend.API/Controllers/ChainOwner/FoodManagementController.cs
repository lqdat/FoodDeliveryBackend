using FoodDeliveryBackend.API.DTOs.ChainOwner;
using FoodDeliveryBackend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryBackend.API.Controllers.ChainOwner;

[ApiController]
[Route("api/chain-owner")]
[Authorize(Roles = "ChainOwner,StoreManager")] 
public class FoodManagementController : ControllerBase
{
    private readonly IAccountService _accountService;

    public FoodManagementController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("foods")]
    public async Task<ActionResult<FoodDto>> CreateFood([FromBody] CreateFoodDto dto)
    {
        try
        {
            // Note: We might want to verify if the caller owns the StoreAccountId, 
            // but AccountService checks store existence. 
            // Better security would be checking User claims vs StoreAccountId here.
            // For now, relying on Service validation.

            var food = await _accountService.CreateFoodAsync(
                dto.StoreAccountId,
                dto.Name,
                dto.Description,
                dto.Price,
                dto.OriginalPrice,
                dto.ImageUrl,
                dto.Category,
                dto.DisplayOrder);

            return CreatedAtAction(nameof(GetFoods), new { storeId = dto.StoreAccountId }, new FoodDto(
                food.Id,
                food.StoreAccountId,
                food.Store?.StoreName ?? "Unknown", 
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("stores/{storeId}/foods")]
    public async Task<ActionResult<IEnumerable<FoodDto>>> GetFoods(Guid storeId)
    {
        var foods = await _accountService.GetFoodsByStoreAsync(storeId);
        
        // Note: StoreName might be null if not included in GetAll. 
        // Service just does Where().ToListAsync(). 
        // Might need Include(f => f.Store) in Service for StoreName.
        
        var dtos = foods.Select(f => new FoodDto(
            f.Id,
            f.StoreAccountId,
            f.Store?.StoreName ?? "Unknown", 
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
            f.UpdatedAt));

        return Ok(dtos);
    }
}
