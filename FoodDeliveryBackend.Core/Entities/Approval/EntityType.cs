namespace FoodDeliveryBackend.Core.Entities.Approval;

/// <summary>
/// Entity types that require approval workflow.
/// Add new values here to extend approval system to new entities.
/// </summary>
public enum EntityType
{
    /// <summary>
    /// Chain owner account registration.
    /// </summary>
    ChainOwner = 1,
    
    /// <summary>
    /// Store account under a chain owner.
    /// </summary>
    StoreAccount = 2,
    
    /// <summary>
    /// Store manager account under a store.
    /// </summary>
    StoreManager = 3,
    
    /// <summary>
    /// Food item created by store.
    /// </summary>
    Food = 4
}
