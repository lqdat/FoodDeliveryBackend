using FoodDeliveryBackend.API.DTOs.Admin;
using FoodDeliveryBackend.API.Services;
using FoodDeliveryBackend.Core.Entities.Admin;
using FoodDeliveryBackend.Core.Entities.Approval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers.Admin;

/// <summary>
/// Controller for Region Admin approval operations.
/// Only accessible by AdminRestaurantRegion role.
/// </summary>
[ApiController]
[Route("api/admin/region")]
[Authorize(Policy = "RegionAdminOnly")]
public class RegionAdminController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public RegionAdminController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    /// <summary>
    /// Get pending approval requests for this region.
    /// </summary>
    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalRequestDto>>> GetPendingApprovals()
    {
        var regionCode = User.FindFirst("RegionCode")?.Value;
        if (string.IsNullOrEmpty(regionCode))
        {
            return BadRequest(new { message = "RegionCode not found in token" });
        }

        var requests = await _approvalService.GetPendingForRegionAsync(regionCode);
        
        var dtos = requests.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get approval request details.
    /// </summary>
    [HttpGet("approvals/{id}")]
    public async Task<ActionResult<ApprovalRequestDto>> GetApprovalById(Guid id)
    {
        var request = await _approvalService.GetByIdAsync(id);
        if (request == null)
        {
            return NotFound(new { message = "Approval request not found" });
        }

        // Verify region access
        var regionCode = User.FindFirst("RegionCode")?.Value;
        if (request.RegionCode != regionCode)
        {
            return Forbid();
        }

        return Ok(MapToDto(request));
    }

    /// <summary>
    /// Approve request (first level approval).
    /// </summary>
    [HttpPost("approvals/{id}/approve")]
    public async Task<ActionResult<ApprovalRequestDto>> Approve(Guid id, [FromBody] ApprovalActionDto dto)
    {
        try
        {
            var adminId = GetCurrentAdminId();
            var regionCode = User.FindFirst("RegionCode")?.Value;

            // Verify region access first
            var request = await _approvalService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { message = "Approval request not found" });
            }
            if (request.RegionCode != regionCode)
            {
                return Forbid();
            }

            var result = await _approvalService.ApproveByRegionAsync(id, adminId, dto.Reason);
            return Ok(MapToDto(result));
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
    /// Reject request (first level rejection).
    /// </summary>
    [HttpPost("approvals/{id}/reject")]
    public async Task<ActionResult<ApprovalRequestDto>> Reject(Guid id, [FromBody] ApprovalActionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return BadRequest(new { message = "Reason is required for rejection" });
        }

        try
        {
            var adminId = GetCurrentAdminId();
            var regionCode = User.FindFirst("RegionCode")?.Value;

            // Verify region access first
            var request = await _approvalService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { message = "Approval request not found" });
            }
            if (request.RegionCode != regionCode)
            {
                return Forbid();
            }

            var result = await _approvalService.RejectByRegionAsync(id, adminId, dto.Reason);
            return Ok(MapToDto(result));
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

    private Guid GetCurrentAdminId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(idClaim!);
    }

    private static ApprovalRequestDto MapToDto(ApprovalRequest request)
    {
        return new ApprovalRequestDto(
            request.Id,
            request.EntityType.ToString(),
            request.EntityId,
            request.CurrentStatus.ToString(),
            request.RegionCode,
            request.CreatedAt,
            request.UpdatedAt,
            request.ApprovalLogs.Select(l => new ApprovalLogDto(
                l.Id,
                l.Action.ToString(),
                l.FromStatus?.ToString(),
                l.ToStatus.ToString(),
                l.PerformedBy,
                l.PerformerRole?.ToString(),
                l.PerformerAccountType,
                l.Reason,
                l.CreatedAt)).ToList());
    }
}
