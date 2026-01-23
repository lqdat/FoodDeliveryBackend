using System.Security.Claims;
using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Core.Entities;
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
        var user = await _context.Users
            .Include(u => u.Customer)
                .ThenInclude(c => c.Addresses)
            .Include(u => u.Customer)
                .ThenInclude(c => c.Orders)
            .Include(u => u.Driver)
            .Include(u => u.Merchant)
                .ThenInclude(m => m.Restaurants)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound("User not found.");

        // Auto-create Profile if missing based on Role
        // 2 = Customer
        if (user.Role == 2 && user.Customer == null)
        {
            user.Customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LoyaltyPoints = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(user.Customer);
            await _context.SaveChangesAsync();
        }
        // 3 = Merchant
        else if (user.Role == 3 && user.Merchant == null)
        {
            user.Merchant = new Merchant
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BusinessName = "New Store",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Merchants.Add(user.Merchant);
            await _context.SaveChangesAsync();
        }
        // 4 = Driver
        else if (user.Role == 4 && user.Driver == null)
        {
            user.Driver = new Driver
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IsOnline = false,
                IsVerified = false,
                VehicleType = "Bike",
                VehiclePlate = "",
                CreatedAt = DateTime.UtcNow
            };
            _context.Drivers.Add(user.Driver);
            await _context.SaveChangesAsync();
        }

        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            DateOfBirth = user.DateOfBirth,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LinkedAccounts = new List<LinkedAccountDto>
            {
                new LinkedAccountDto { Provider = "Google", IsLinked = !string.IsNullOrEmpty(user.GoogleId) },
                new LinkedAccountDto { Provider = "Facebook", IsLinked = !string.IsNullOrEmpty(user.FacebookId) }
            }
        };
        
        // Customer
        if (user.Role == 2 && user.Customer != null)
        {
            dto.LoyaltyPoints = user.Customer.LoyaltyPoints;
            dto.AddressCount = user.Customer.Addresses.Count(a => !a.IsDeleted);
            dto.OrderCount = user.Customer.Orders.Count;
        }

        // Merchant
        if (user.Role == 3 && user.Merchant != null)
        {
            dto.BusinessName = user.Merchant.BusinessName;
            dto.IsMerchantActive = user.Merchant.IsActive;
            dto.RestaurantCount = user.Merchant.Restaurants.Count(r => !r.IsDeleted);
        }

        // Driver
        if (user.Role == 4 && user.Driver != null)
        {
            dto.VehicleType = user.Driver.VehicleType;
            dto.VehiclePlate = user.Driver.VehiclePlate;
            dto.IsOnline = user.Driver.IsOnline;
            dto.WalletBalance = user.Driver.WalletBalance;
            dto.DriverRating = user.Driver.Rating;
            dto.OrderCount = user.Driver.TotalDeliveries; 
        }

        return dto;
    }

    // PUT: api/users/profile
    // PUT: api/users/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        // Load User with relations to update them if needed
        var user = await _context.Users
            .Include(u => u.Driver)
            .Include(u => u.Merchant)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound("User not found.");

        // 1. Update Common User Info
        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Email = request.Email;
        user.AvatarUrl = request.AvatarUrl; // Updates Avatar for everyone
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth.Value;
        user.UpdatedAt = DateTime.UtcNow;

        // 2. Update Driver Specifics
        if (user.Role == 4 && user.Driver != null)
        {
            if (!string.IsNullOrEmpty(request.VehicleType)) user.Driver.VehicleType = request.VehicleType;
            if (!string.IsNullOrEmpty(request.VehiclePlate)) user.Driver.VehiclePlate = request.VehiclePlate;
        }

        // 3. Update Merchant Specifics
        if (user.Role == 3 && user.Merchant != null)
        {
             if (!string.IsNullOrEmpty(request.BusinessName)) user.Merchant.BusinessName = request.BusinessName;
        }

        await _context.SaveChangesAsync();

        // Return updated profile (reuse GetProfile logic or simpler return)
        // For simplicity/consistency, let's just return success message. 
        // Frontend usually re-fetches profile or updates local state.
        return Ok(new { message = "Profile updated successfully" });
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
    // POST: api/users/avatar
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");

        // Define path
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        // Unique filename
        var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Construct URL (assuming default local dev, in Prod use CDN or proper Host handling)
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var avatarUrl = $"{baseUrl}/uploads/{fileName}";

        // Update DB
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { avatarUrl });
    }
}
