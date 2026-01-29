using FoodDeliveryBackend.API.DTOs.ChainOwner;
using FoodDeliveryBackend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers.ChainOwner;

[ApiController]
[Route("api/chain-owner")]
[Authorize(Roles = "ChainOwner")]
public class StoreManagementController : ControllerBase
{
    private readonly IAccountService _accountService;

    public StoreManagementController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("stores")]
    public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreDto dto)
    {
        try
        {
            var chainOwnerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var store = await _accountService.CreateStoreAsync(
                chainOwnerId,
                dto.StoreName,
                dto.Description,
                dto.Address,
                dto.Latitude,
                dto.Longitude,
                dto.PhoneNumber,
                dto.RegionCode);

            return CreatedAtAction(nameof(GetStores), new { }, new StoreDto(
                store.Id,
                store.StoreName,
                store.Description,
                store.Address,
                store.Latitude,
                store.Longitude,
                store.PhoneNumber,
                store.RegionCode,
                store.Status.ToString(),
                store.IsOpen,
                store.CreatedAt,
                0, 0));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("stores")]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
    {
        var chainOwnerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var stores = await _accountService.GetStoresByChainOwnerAsync(chainOwnerId);

        var dtos = stores.Select(s => new StoreDto(
            s.Id,
            s.StoreName,
            s.Description,
            s.Address,
            s.Latitude,
            s.Longitude,
            s.PhoneNumber,
            s.RegionCode,
            s.Status.ToString(),
            s.IsOpen,
            s.CreatedAt,
            s.Managers?.Count ?? 0,
            s.Foods?.Count ?? 0));

        return Ok(dtos);
    }
}
