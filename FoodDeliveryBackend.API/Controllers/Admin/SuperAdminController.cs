using FoodDeliveryBackend.API.DTOs.Admin;
using FoodDeliveryBackend.API.Services;
using FoodDeliveryBackend.Core.Entities.Approval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryBackend.API.Controllers.Admin;

/// <summary>
/// Controller for Super Admin operations.
/// Super Admin can ONLY view audit logs - NO approve/reject access.
/// </summary>
[ApiController]
[Route("api/super-admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class SuperAdminController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public SuperAdminController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    /// <summary>
    /// Get audit logs with pagination and filtering.
    /// This is the ONLY operation Super Admin can perform.
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<ActionResult<AuditLogListResponseDto>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entityType = null,
        [FromQuery] string? regionCode = null)
    {
        EntityType? entityTypeFilter = null;
        if (!string.IsNullOrEmpty(entityType) && Enum.TryParse<EntityType>(entityType, out var parsed))
        {
            entityTypeFilter = parsed;
        }

        var (logs, totalCount) = await _approvalService.GetAuditLogsAsync(
            page,
            pageSize,
            entityTypeFilter,
            regionCode);

        var dtos = logs.Select(l => new AuditLogDto(
            l.Id,
            l.ApprovalRequestId,
            l.ApprovalRequest.EntityType.ToString(),
            l.ApprovalRequest.EntityId,
            l.ApprovalRequest.RegionCode,
            l.Action.ToString(),
            l.FromStatus?.ToString(),
            l.ToStatus.ToString(),
            l.PerformedBy,
            l.PerformerRole?.ToString(),
            l.PerformerAccountType,
            l.Reason,
            l.CreatedAt)).ToList();

        return Ok(new AuditLogListResponseDto(dtos, totalCount, page, pageSize));
    }
}
