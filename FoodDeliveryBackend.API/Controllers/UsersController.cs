using System.Security.Claims;
using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public UsersController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId != null ? Guid.Parse(userId) : Guid.Empty;
    }

    // GET: api/users/profile
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound("User not found.");

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    // PUT: api/users/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound("User not found.");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully", user = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        }});
    }

    // POST: api/users/change-password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound("User not found.");

        // Simple check (In production use BCrypt)
        if (user.PasswordHash != request.CurrentPassword && user.PasswordHash != "hashed_password_" + request.CurrentPassword)
        {
            return BadRequest("Current password is incorrect.");
        }

        user.PasswordHash = request.NewPassword;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }
}
