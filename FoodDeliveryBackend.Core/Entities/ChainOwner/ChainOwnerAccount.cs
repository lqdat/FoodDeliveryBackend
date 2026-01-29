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

    // ========================================================================
    // DIGITAL SIGNATURE & CONTRACT FIELDS
    // ========================================================================

    /// <summary>
    /// Unique contract number (e.g., HD-2026-001234).
    /// </summary>
    public string? ContractNumber { get; set; }

    /// <summary>
    /// URL to the original contract PDF (unsigned).
    /// </summary>
    public string? ContractPdfUrl { get; set; }

    /// <summary>
    /// URL to the signed contract PDF.
    /// </summary>
    public string? SignedPdfUrl { get; set; }

    /// <summary>
    /// URL to the signature image (handwritten representation).
    /// </summary>
    public string? SignatureImageUrl { get; set; }

    /// <summary>
    /// Timestamp when the digital signature was applied.
    /// </summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>
    /// IP address of the signer.
    /// </summary>
    public string? SignerIp { get; set; }

    /// <summary>
    /// Device information of the signer.
    /// </summary>
    public string? SignerDevice { get; set; }

    /// <summary>
    /// Whether OTP verification was successful for this signature.
    /// </summary>
    public bool OtpVerified { get; set; } = false;

    /// <summary>
    /// SHA256 hash of the document content at time of signing.
    /// </summary>
    public string? DocumentHash { get; set; }

    /// <summary>
    /// Unique ID for the signature transaction.
    /// </summary>
    public string? SignatureId { get; set; }
}
