namespace FoodDeliveryBackend.Core.Entities.Admin;

/// <summary>
/// Admin account entity - completely separate from User entity.
/// Used for restaurant/food approval workflow management.
/// </summary>
public class AdminAccount
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string PasswordHash { get; set; } = null!;
    
    public string FullName { get; set; } = null!;
    
    /// <summary>
    /// Admin role determining approval capabilities.
    /// </summary>
    public AdminRole Role { get; set; }
    
    /// <summary>
    /// Region code for RegionAdmin (e.g., "HN", "SGN", "DN").
    /// NULL for SuperAdmin and MasterAdmin (they have global access).
    /// </summary>
    public string? RegionCode { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
}
