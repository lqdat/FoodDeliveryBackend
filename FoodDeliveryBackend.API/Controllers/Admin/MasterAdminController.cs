using FoodDeliveryBackend.API.DTOs.Admin;
using FoodDeliveryBackend.API.Services;
using FoodDeliveryBackend.Core.Entities.Approval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers.Admin;

/// <summary>
/// Controller for Master Admin approval operations.
/// Only accessible by AdminRestaurantMaster role.
/// </summary>
[ApiController]
[Route("api/admin/master")]
[Authorize(Policy = "MasterAdminOnly")]
public class MasterAdminController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public MasterAdminController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    /// <summary>
    /// Get requests pending master approval (already approved by region).
    /// </summary>
    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalRequestDto>>> GetPendingApprovals()
    {
        var requests = await _approvalService.GetPendingForMasterAsync();
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

        return Ok(MapToDto(request));
    }

    /// <summary>
    /// Final approval (second level).
    /// </summary>
    [HttpPost("approvals/{id}/approve")]
    public async Task<ActionResult<ApprovalRequestDto>> Approve(Guid id, [FromBody] ApprovalActionDto dto)
    {
        try
        {
            var adminId = GetCurrentAdminId();
            var result = await _approvalService.ApproveByMasterAsync(id, adminId, dto.Reason);
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
    /// Final rejection (second level).
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
            var result = await _approvalService.RejectByMasterAsync(id, adminId, dto.Reason);
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
