using System.Collections.Generic;

namespace FoodDeliveryBackend.API.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int Role { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Linked Accounts Status
    public List<LinkedAccountDto> LinkedAccounts { get; set; } = new();

    // Customer specific
    public int? LoyaltyPoints { get; set; }
    public int? AddressCount { get; set; }
    public int? OrderCount { get; set; }

    // Driver Specific
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public bool? IsOnline { get; set; }
    public decimal? WalletBalance { get; set; }
    public double? DriverRating { get; set; }

    // Merchant Specific
    public string? BusinessName { get; set; }
    public int? RestaurantCount { get; set; }
    public bool? IsMerchantActive { get; set; }
}

public class LinkedAccountDto
{
    public string Provider { get; set; } = null!; // "Google", "Facebook"
    public bool IsLinked { get; set; }
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Driver Update
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }

    // Merchant Update
    public string? BusinessName { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

// Settings Screen DTOs
public class UserSettingsDto
{
    // Account Section
    public SecuritySettingsDto Security { get; set; } = new();
    public PrivacySettingsDto Privacy { get; set; } = new();
    
    // General Settings Section
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool PromoEmailsEnabled { get; set; } = false;
    public string Language { get; set; } = "vi"; // vi, en
    public string LanguageDisplay { get; set; } = "Tiếng Việt";
    public string Theme { get; set; } = "system"; // system, light, dark
    public string ThemeDisplay { get; set; } = "Hệ thống";
    
    // App Info
    public string AppVersion { get; set; } = "4.20.1";
    public string BuildNumber { get; set; } = "892";
}

public class SecuritySettingsDto
{
    public bool HasPassword { get; set; } = true;
    public bool FaceIdEnabled { get; set; } = false;
    public bool FingerprintEnabled { get; set; } = false;
    public DateTime? LastPasswordChange { get; set; }
}

public class PrivacySettingsDto
{
    public bool AllowDataCollection { get; set; } = true;
    public bool AllowLocationTracking { get; set; } = true;
    public bool AllowPersonalizedAds { get; set; } = true;
}

public class UpdateSettingsRequest
{
    public bool? PushNotificationsEnabled { get; set; }
    public bool? PromoEmailsEnabled { get; set; }
    public string? Language { get; set; }
    public string? Theme { get; set; }
    public bool? FaceIdEnabled { get; set; }
    public bool? FingerprintEnabled { get; set; }
    public bool? AllowDataCollection { get; set; }
    public bool? AllowLocationTracking { get; set; }
    public bool? AllowPersonalizedAds { get; set; }
}
