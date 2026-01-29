using FoodDeliveryBackend.API.DTOs.ChainOwner;
using FoodDeliveryBackend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers.ChainOwner;

[ApiController]
[Route("api/chain-owner")]
[Authorize(Roles = "ChainOwner")]
public class ManagerManagementController : ControllerBase
{
    private readonly IAccountService _accountService;

    public ManagerManagementController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("managers")]
    public async Task<ActionResult<ManagerDto>> CreateManager([FromBody] CreateManagerDto dto)
    {
        try
        {
            var chainOwnerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var manager = await _accountService.CreateManagerAsync(
                chainOwnerId,
                dto.StoreAccountId,
                dto.Email,
                dto.Password,
                dto.FullName,
                dto.PhoneNumber);

            return CreatedAtAction(nameof(CreateManager), new { id = manager.Id }, new ManagerDto(
                manager.Id,
                manager.StoreAccountId,
                manager.Store?.StoreName ?? "Unknown",
                manager.Email,
                manager.FullName,
                manager.PhoneNumber,
                manager.Status.ToString(),
                manager.CreatedAt));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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
}
