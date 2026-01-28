using FoodDeliveryBackend.Core.Entities.Admin;

namespace FoodDeliveryBackend.Core.Entities.Approval;

/// <summary>
/// Central approval request entity - tracks approval state for any entity type.
/// Enables generic approval workflow without entity-specific tables.
/// </summary>
public class ApprovalRequest
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Type of entity being approved (ChainOwner, Store, Manager, Food).
    /// </summary>
    public EntityType EntityType { get; set; }
    
    /// <summary>
    /// ID of the entity being approved.
    /// References the specific entity based on EntityType.
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Current approval status in the workflow.
    /// </summary>
    public ApprovalStatus CurrentStatus { get; set; } = ApprovalStatus.Submitted;
    
    /// <summary>
    /// Region code for routing to appropriate region admin.
    /// </summary>
    public string RegionCode { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Audit trail of all approval actions.
    /// </summary>
    public virtual ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
}
