using FoodDeliveryBackend.Core.Entities.Admin;

namespace FoodDeliveryBackend.Core.Entities.Approval;

/// <summary>
/// Immutable audit log for approval actions.
/// APPEND-ONLY: No updates or deletes allowed.
/// </summary>
public class ApprovalLog
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to the approval request.
    /// </summary>
    public Guid ApprovalRequestId { get; set; }
    
    /// <summary>
    /// Action performed (Submit, Approve, Reject).
    /// </summary>
    public ApprovalAction Action { get; set; }
    
    /// <summary>
    /// Status before this action. NULL for initial submission.
    /// </summary>
    public ApprovalStatus? FromStatus { get; set; }
    
    /// <summary>
    /// Status after this action.
    /// </summary>
    public ApprovalStatus ToStatus { get; set; }
    
    /// <summary>
    /// Admin who performed the action.
    /// For Submit action, this is the entity owner (ChainOwner/StoreManager).
    /// </summary>
    public Guid PerformedBy { get; set; }
    
    /// <summary>
    /// Role of the performer at time of action.
    /// Stored as int to preserve historical accuracy even if role changes.
    /// </summary>
    public AdminRole? PerformerRole { get; set; }
    
    /// <summary>
    /// Account type if performer is not an admin (e.g., "ChainOwner", "StoreManager").
    /// </summary>
    public string? PerformerAccountType { get; set; }
    
    /// <summary>
    /// Reason for rejection or approval notes.
    /// Required for rejections.
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Timestamp of action. Immutable once created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ApprovalRequest ApprovalRequest { get; set; } = null!;
}
