namespace FoodDeliveryBackend.Core.Entities.ChainOwner;

/// <summary>
/// Account status for ChainOwner, StoreAccount, and StoreManager.
/// Mapped from ApprovalStatus upon final approval/rejection.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// Awaiting approval. Cannot perform business operations.
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Fully approved and active. Can perform all allowed operations.
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Temporarily suspended by admin.
    /// </summary>
    Suspended = 2,
    
    /// <summary>
    /// Rejected during approval process.
    /// </summary>
    Rejected = 3
}
