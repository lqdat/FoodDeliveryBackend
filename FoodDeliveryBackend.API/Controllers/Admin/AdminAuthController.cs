using FoodDeliveryBackend.API.DTOs.Admin;
using FoodDeliveryBackend.API.Services;
using FoodDeliveryBackend.Core.Entities.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodDeliveryBackend.API.Controllers.Admin;

[ApiController]
[Route("api/auth/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly string _jwtSecret;

    public AdminAuthController(IAdminService adminService)
    {
        _adminService = adminService;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? "super_secret_key_change_this_in_production_1234567890";
    }

    /// <summary>
    /// Admin login endpoint.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AdminLoginResponseDto>> Login([FromBody] AdminLoginDto dto)
    {
        var admin = await _adminService.AuthenticateAsync(dto.Email, dto.Password);
        if (admin == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = GenerateJwtToken(admin);

        return Ok(new AdminLoginResponseDto(
            admin.Id,
            admin.Email,
            admin.FullName,
            admin.Role.ToString(),
            admin.RegionCode,
            token));
    }

    private string GenerateJwtToken(AdminAccount admin)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new(ClaimTypes.Email, admin.Email),
            new(ClaimTypes.Name, admin.FullName),
            new(ClaimTypes.Role, admin.Role.ToString()),
            new("AccountType", "Admin")
        };

        if (!string.IsNullOrEmpty(admin.RegionCode))
        {
            claims.Add(new Claim("RegionCode", admin.RegionCode));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
