using FoodDeliveryBackend.Core.Entities.Approval;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Generic approval service interface for multi-tier approval workflow.
/// Handles all entity types through a single unified API.
/// </summary>
public interface IApprovalService
{
    /// <summary>
    /// Submit an entity for approval. Creates ApprovalRequest and initial log entry.
    /// </summary>
    Task<ApprovalRequest> SubmitForApprovalAsync(
        EntityType entityType, 
        Guid entityId, 
        string regionCode,
        Guid submitterId,
        string submitterAccountType);

    /// <summary>
    /// Get pending approval requests for a region admin.
    /// </summary>
    Task<IEnumerable<ApprovalRequest>> GetPendingForRegionAsync(string regionCode);

    /// <summary>
    /// Get approval requests that passed region approval, pending master review.
    /// </summary>
    Task<IEnumerable<ApprovalRequest>> GetPendingForMasterAsync();

    /// <summary>
    /// Region admin approves the request.
    /// </summary>
    Task<ApprovalRequest> ApproveByRegionAsync(Guid requestId, Guid adminId, string? reason = null);

    /// <summary>
    /// Region admin rejects the request.
    /// </summary>
    Task<ApprovalRequest> RejectByRegionAsync(Guid requestId, Guid adminId, string reason);

    /// <summary>
    /// Master admin approves the request. Triggers entity activation.
    /// </summary>
    Task<ApprovalRequest> ApproveByMasterAsync(Guid requestId, Guid adminId, string? reason = null);

    /// <summary>
    /// Master admin rejects the request.
    /// </summary>
    Task<ApprovalRequest> RejectByMasterAsync(Guid requestId, Guid adminId, string reason);

    /// <summary>
    /// Get approval request by ID with logs.
    /// </summary>
    Task<ApprovalRequest?> GetByIdAsync(Guid requestId);

    /// <summary>
    /// Get audit logs with pagination.
    /// </summary>
    Task<(IEnumerable<ApprovalLog> Logs, int TotalCount)> GetAuditLogsAsync(
        int page = 1, 
        int pageSize = 50,
        EntityType? entityTypeFilter = null,
        string? regionCodeFilter = null);
}
