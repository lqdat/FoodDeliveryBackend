namespace FoodDeliveryBackend.API.DTOs;

public class LoginRequest
{
    public string PhoneNumber { get; set; } = null!;
}

public class VerifyOtpRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string Otp { get; set; } = null!;
}

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public int Role { get; set; }
    public string Token { get; set; } = null!;
}
