namespace FoodDeliveryBackend.Core.Entities.ChainOwner;

/// <summary>
/// Chain Owner - top level of restaurant account hierarchy.
/// Can own multiple stores. Requires approval to become active.
/// </summary>
public class ChainOwnerAccount
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string PasswordHash { get; set; } = null!;
    
    public string FullName { get; set; } = null!;
    
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Business/company name.
    /// </summary>
    public string BusinessName { get; set; } = null!;
    
    /// <summary>
    /// Business registration number for verification.
    /// </summary>
    public string? BusinessRegistrationNumber { get; set; }
    
    /// <summary>
    /// Region code for approval routing (e.g., "HN", "SGN").
    /// </summary>
    public string RegionCode { get; set; } = null!;
    
    /// <summary>
    /// Current account status.
    /// </summary>
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Stores owned by this chain owner.
    /// </summary>
    public virtual ICollection<StoreAccount> Stores { get; set; } = new List<StoreAccount>();
}
