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

    private readonly IConfiguration _configuration;

    public AuthController(FoodDeliveryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login-otp")]
    public async Task<IActionResult> LoginOtp([FromBody] LoginRequest request)
    {
        // ... same logic ...
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (user == null) return NotFound(new { message = "User not found with this phone number." });
        return Ok(new { success = true, message = "OTP sent to " + request.PhoneNumber, dev_hint = "Use 123456" });
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (user == null) return NotFound("User not found.");

        // Simulate OTP check (Mock)
        // if (request.Otp != "123456") ...

        // Generate Real JWT Token
        var token = GenerateJwtToken(user);

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

    [HttpPost("login-password")]
    public async Task<ActionResult<AuthResponse>> LoginPassword([FromBody] LoginPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return NotFound(new { message = "User not found with this email." });

        // Simple password check (In production, use BCrypt.Verify)
        if (user.PasswordHash != request.Password && user.PasswordHash != "hashed_password_" + request.Password)
        {
            return Unauthorized(new { message = "Invalid password." });
        }

        var token = GenerateJwtToken(user);

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

    private string GenerateJwtToken(FoodDeliveryBackend.Core.Entities.User user)
    {
        var jwtSecret = _configuration["JWT_SECRET"] ?? "super_secret_key_change_this_in_production_1234567890";
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.MobilePhone, user.PhoneNumber),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
