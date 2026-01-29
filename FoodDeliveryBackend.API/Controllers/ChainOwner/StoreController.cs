using FoodDeliveryBackend.API.DTOs.ChainOwner;
using FoodDeliveryBackend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers.ChainOwner;

/// <summary>
/// Controller for Chain Owner operations.
/// Requires ChainOwner authentication and Active status for most operations.
/// </summary>
[ApiController]
[Route("api/chain-owner")]
[Authorize(Policy = "ChainOwnerOnly")]
public class StoreController : ControllerBase
{
    private readonly IAccountService _accountService;

    public StoreController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Get current Chain Owner profile with stores.
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<ChainOwnerProfileDto>> GetProfile()
    {
        var chainOwnerId = GetCurrentAccountId();
        var chainOwner = await _accountService.GetChainOwnerByIdAsync(chainOwnerId);
        
        if (chainOwner == null)
        {
            return NotFound(new { message = "Chain Owner not found" });
        }

        return Ok(MapToProfileDto(chainOwner));
    }

    /// <summary>
    /// Create a new store under this Chain Owner.
    /// Chain Owner must be Active to create stores.
    /// </summary>
    [HttpPost("stores")]
    public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreDto dto)
    {
        try
        {
            var chainOwnerId = GetCurrentAccountId();
            var store = await _accountService.CreateStoreAsync(
                chainOwnerId,
                dto.StoreName,
                dto.Description,
                dto.Address,
                dto.Latitude,
                dto.Longitude,
                dto.PhoneNumber,
                dto.RegionCode);

            var response = new StoreDto(
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
                0,
                0);

            return CreatedAtAction(nameof(GetStores), response);
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
    /// Get all stores for this Chain Owner.
    /// </summary>
    [HttpGet("stores")]
    public async Task<ActionResult<List<StoreDto>>> GetStores()
    {
        var chainOwnerId = GetCurrentAccountId();
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
            s.Foods?.Count ?? 0)).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Create a new Store Manager.
    /// Store must be Active to create managers.
    /// </summary>
    [HttpPost("managers")]
    public async Task<ActionResult<ManagerDto>> CreateManager([FromBody] CreateManagerDto dto)
    {
        try
        {
            var chainOwnerId = GetCurrentAccountId();
            var manager = await _accountService.CreateManagerAsync(
                chainOwnerId,
                dto.StoreAccountId,
                dto.Email,
                dto.Password,
                dto.FullName,
                dto.PhoneNumber);

            var response = new ManagerDto(
                manager.Id,
                manager.StoreAccountId,
                manager.Store?.StoreName ?? "",
                manager.Email,
                manager.FullName,
                manager.PhoneNumber,
                manager.Status.ToString(),
                manager.CreatedAt);

            return CreatedAtAction(nameof(CreateManager), response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentAccountId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(idClaim!);
    }

    private static ChainOwnerProfileDto MapToProfileDto(FoodDeliveryBackend.Core.Entities.ChainOwner.ChainOwnerAccount chainOwner)
    {
        return new ChainOwnerProfileDto(
            chainOwner.Id,
            chainOwner.Email,
            chainOwner.FullName,
            chainOwner.PhoneNumber,
            chainOwner.BusinessName,
            chainOwner.BusinessRegistrationNumber,
            chainOwner.RegionCode,
            chainOwner.Status.ToString(),
            chainOwner.CreatedAt,
            // Signature fields
            chainOwner.ContractNumber,
            chainOwner.SignedPdfUrl,
            chainOwner.SignedAt,
            chainOwner.SignatureId,
            chainOwner.Stores?.Select(s => new StoreDto(
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
                s.Foods?.Count ?? 0)).ToList() ?? new List<StoreDto>());
    }
}
