namespace FoodDeliveryBackend.Core.Entities.ChainOwner;

/// <summary>
/// Food item status.
/// </summary>
public enum FoodStatus
{
    /// <summary>
    /// Draft - not yet submitted for approval.
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Submitted and awaiting approval.
    /// </summary>
    PendingApproval = 1,
    
    /// <summary>
    /// Approved and visible to customers.
    /// </summary>
    Published = 2,
    
    /// <summary>
    /// Rejected during approval.
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// Temporarily unavailable.
    /// </summary>
    Unavailable = 4
}

/// <summary>
/// Food item - menu item created by store for approval.
/// Separate from MenuItem entity used in customer ordering flow.
/// Requires approval workflow before becoming Published.
/// </summary>
public class Food
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Parent store that owns this food item.
    /// </summary>
    public Guid StoreAccountId { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public decimal? OriginalPrice { get; set; }
    
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Category within the store menu.
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Current food status.
    /// Cannot be Published if StoreAccount is not Active.
    /// </summary>
    public FoodStatus Status { get; set; } = FoodStatus.Draft;
    
    public bool IsAvailable { get; set; } = true;
    
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public virtual StoreAccount Store { get; set; } = null!;
}
