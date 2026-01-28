namespace FoodDeliveryBackend.Core.Entities.Admin;

/// <summary>
/// Admin role hierarchy for restaurant approval system.
/// Higher value = higher privilege level.
/// </summary>
public enum AdminRole
{
    /// <summary>
    /// Region-level admin. Can approve/reject entities within their region.
    /// First level of approval workflow.
    /// </summary>
    AdminRestaurantRegion = 1,
    
    /// <summary>
    /// Master-level admin. Can approve/reject entities after region approval.
    /// Second (final) level of approval workflow.
    /// </summary>
    AdminRestaurantMaster = 2,
    
    /// <summary>
    /// Super admin. READ-ONLY access to audit logs.
    /// CANNOT approve or reject any entity.
    /// </summary>
    SuperAdmin = 3
}
