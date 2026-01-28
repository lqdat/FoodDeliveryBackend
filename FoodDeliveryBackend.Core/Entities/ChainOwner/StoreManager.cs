namespace FoodDeliveryBackend.Core.Entities.ChainOwner;

/// <summary>
/// Store Manager - operational staff account under a store.
/// Can manage food items and handle store operations.
/// Requires StoreAccount to be Active before becoming Active.
/// </summary>
public class StoreManager
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Parent store.
    /// </summary>
    public Guid StoreAccountId { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string PasswordHash { get; set; } = null!;
    
    public string FullName { get; set; } = null!;
    
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Current account status.
    /// Cannot be Active if StoreAccount is not Active.
    /// </summary>
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation
    public virtual StoreAccount Store { get; set; } = null!;
}
