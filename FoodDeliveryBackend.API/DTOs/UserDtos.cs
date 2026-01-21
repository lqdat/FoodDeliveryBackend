namespace FoodDeliveryBackend.API.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public int Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
