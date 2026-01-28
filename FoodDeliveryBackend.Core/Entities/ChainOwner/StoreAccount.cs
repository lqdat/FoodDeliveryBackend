namespace FoodDeliveryBackend.Core.Entities.ChainOwner;

/// <summary>
/// Store Account - individual store under a chain owner.
/// Has physical location and can have multiple managers.
/// Requires ChainOwner to be Active before becoming Active.
/// </summary>
public class StoreAccount
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Parent chain owner.
    /// </summary>
    public Guid ChainOwnerId { get; set; }
    
    public string StoreName { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public string Address { get; set; } = null!;
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Region code (inherited from ChainOwner or overridden).
    /// </summary>
    public string RegionCode { get; set; } = null!;
    
    /// <summary>
    /// Current account status.
    /// Cannot be Active if ChainOwner is not Active.
    /// </summary>
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    
    public bool IsOpen { get; set; } = true;
    
    public TimeSpan? OpenTime { get; set; }
    
    public TimeSpan? CloseTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public virtual ChainOwnerAccount ChainOwner { get; set; } = null!;
    
    public virtual ICollection<StoreManager> Managers { get; set; } = new List<StoreManager>();
    
    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();
}
