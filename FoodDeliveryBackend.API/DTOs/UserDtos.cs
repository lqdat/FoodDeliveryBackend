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
