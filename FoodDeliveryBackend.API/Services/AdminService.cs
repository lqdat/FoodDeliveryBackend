using FoodDeliveryBackend.Core.Entities.Admin;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Service interface for Admin account management.
/// </summary>
public interface IAdminService
{
    Task<AdminAccount?> AuthenticateAsync(string email, string password);
    Task<AdminAccount?> GetByIdAsync(Guid id);
    Task<AdminAccount> CreateAdminAsync(
        string email,
        string password,
        string fullName,
        AdminRole role,
        string? regionCode = null);
}

/// <summary>
/// Admin service implementation.
/// </summary>
public class AdminService : IAdminService
{
    private readonly FoodDeliveryDbContext _context;

    public AdminService(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAccount?> AuthenticateAsync(string email, string password)
    {
        var admin = await _context.AdminAccounts
            .FirstOrDefaultAsync(a => a.Email == email && a.IsActive);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            return null;
        }

        admin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return admin;
    }

    public async Task<AdminAccount?> GetByIdAsync(Guid id)
    {
        return await _context.AdminAccounts.FindAsync(id);
    }

    public async Task<AdminAccount> CreateAdminAsync(
        string email,
        string password,
        string fullName,
        AdminRole role,
        string? regionCode = null)
    {
        // Validate region code for region admins
        if (role == AdminRole.AdminRestaurantRegion && string.IsNullOrWhiteSpace(regionCode))
        {
            throw new ArgumentException("RegionCode is required for Region Admin");
        }

        var existing = await _context.AdminAccounts
            .FirstOrDefaultAsync(a => a.Email == email);
        if (existing != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        var admin = new AdminAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            Role = role,
            RegionCode = role == AdminRole.AdminRestaurantRegion 
                ? regionCode?.ToUpperInvariant() 
                : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminAccounts.Add(admin);
        await _context.SaveChangesAsync();

        return admin;
    }
}
