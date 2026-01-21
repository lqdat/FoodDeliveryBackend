using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public AuthController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    [HttpPost("login-otp")]
    public async Task<IActionResult> LoginOtp([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        
        // For security, usually we don't say if user exists, but for this Mock/Dev app:
        if (user == null)
        {
             return NotFound(new { message = "User not found with this phone number." });
        }

        // Simulate Sending OTP
        // In real world: SMSProvider.Send(user.PhoneNumber, "123456");
        
        return Ok(new { success = true, message = "OTP sent to " + request.PhoneNumber, dev_hint = "Use 123456" });
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        // "Just check phone number" logic
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (user == null) return NotFound("User not found.");

        // Simulate OTP check: Any OTP is accepted if user exists
        // if (request.Otp != "123456") ...

        // Generate Mock Token
        var token = "mock-jwt-token-" + Guid.NewGuid();

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            Token = token
        };
    }
}
