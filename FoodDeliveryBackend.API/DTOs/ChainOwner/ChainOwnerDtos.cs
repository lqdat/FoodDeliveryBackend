using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryBackend.API.DTOs.ChainOwner;

// ==================== Auth DTOs ====================

public record RegisterChainOwnerDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    [Required] string FullName,
    string? PhoneNumber,
    [Required] string BusinessName,
    string? BusinessRegistrationNumber,
    [Required][StringLength(10, MinimumLength = 2)] string RegionCode);

public record ChainOwnerLoginDto(
    [Required] string Email,
    [Required] string Password);

public record ChainOwnerLoginResponseDto(
    Guid Id,
    string Email,
    string FullName,
    string BusinessName,
    string Status,
    string RegionCode,
    string Token);

public record ChainOwnerProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    string BusinessName,
    string? BusinessRegistrationNumber,
    string RegionCode,
    string Status,
    DateTime CreatedAt,
    // Signature Info
    string? ContractNumber,
    string? SignedPdfUrl,
    DateTime? SignedAt,
    string? SignatureId,
    List<StoreDto> Stores);

// ==================== Registration Flow DTOs ====================

public record UploadDocsDto(
    string? BusinessLicenseUrl,
    string? IdCardFrontUrl,
    string? IdCardBackUrl,
    string? FoodSafetyCertUrl);

public record ContractViewDto(
    string ContractNumber,
    string ContractContent, // Or URL
    DateTime GeneratedAt);

public record SignContractDto(
    [Required] string ContractNumber,
    [Required] string SignatureImageUrl,
    [Required] string OtpCode,
    string? DeviceInfo,
    string? IpAddress,
    bool AgreedToTerms);

// ==================== Store DTOs ====================

public record CreateStoreDto(
    [Required] string StoreName,
    string? Description,
    [Required] string Address,
    [Required] double Latitude,
    [Required] double Longitude,
    string? PhoneNumber,
    string? RegionCode);

public record StoreDto(
    Guid Id,
    string StoreName,
    string? Description,
    string Address,
    double Latitude,
    double Longitude,
    string? PhoneNumber,
    string RegionCode,
    string Status,
    bool IsOpen,
    DateTime CreatedAt,
    int ManagerCount,
    int FoodCount);

// ==================== Manager DTOs ====================

public record CreateManagerDto(
    [Required] Guid StoreAccountId,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    [Required] string FullName,
    string? PhoneNumber);

public record ManagerDto(
    Guid Id,
    Guid StoreAccountId,
    string StoreName,
    string Email,
    string FullName,
    string? PhoneNumber,
    string Status,
    DateTime CreatedAt);

public record ManagerLoginDto(
    [Required] string Email,
    [Required] string Password);

public record ManagerLoginResponseDto(
    Guid Id,
    string Email,
    string FullName,
    Guid StoreAccountId,
    string StoreName,
    string Status,
    string Token);

// ==================== Food DTOs ====================

public record CreateFoodDto(
    [Required] Guid StoreAccountId,
    [Required] string Name,
    string? Description,
    [Required][Range(0, double.MaxValue)] decimal Price,
    decimal? OriginalPrice,
    string? ImageUrl,
    string? Category,
    int DisplayOrder = 0);

public record FoodDto(
    Guid Id,
    Guid StoreAccountId,
    string StoreName,
    string Name,
    string? Description,
    decimal Price,
    decimal? OriginalPrice,
    string? ImageUrl,
    string? Category,
    string Status,
    bool IsAvailable,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpdateFoodDto(
    string? Name,
    string? Description,
    decimal? Price,
    decimal? OriginalPrice,
    string? ImageUrl,
    string? Category,
    int? DisplayOrder,
    bool? IsAvailable);
