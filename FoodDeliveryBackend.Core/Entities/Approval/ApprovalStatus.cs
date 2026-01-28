namespace FoodDeliveryBackend.Core.Entities.Approval;

/// <summary>
/// Approval workflow status - shared across all approvable entities.
/// Represents the state machine for multi-tier approval.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Initial state when entity is submitted for approval.
    /// </summary>
    Submitted = 0,
    
    /// <summary>
    /// Rejected by region admin. Terminal state.
    /// </summary>
    RejectedByRegion = 1,
    
    /// <summary>
    /// Approved by region admin. Awaiting master admin review.
    /// </summary>
    ApprovedByRegion = 2,
    
    /// <summary>
    /// Rejected by master admin. Terminal state.
    /// </summary>
    RejectedByMaster = 3,
    
    /// <summary>
    /// Fully approved by master admin. Terminal state.
    /// Entity becomes ACTIVE/PUBLISHED.
    /// </summary>
    Approved = 4
}
