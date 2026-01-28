using FoodDeliveryBackend.Core.Entities.Admin;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Seeder for Admin accounts.
/// Creates initial Super Admin if none exists.
/// </summary>
public static class AdminSeeder
{
    /// <summary>
    /// Seeds the database with initial admin accounts if they don't exist.
    /// </summary>
    public static async Task SeedAdminAccountsAsync(FoodDeliveryDbContext context)
    {
        // Check if any admin exists
        if (await context.AdminAccounts.AnyAsync())
        {
            Console.WriteLine("[AdminSeeder] Admin accounts already exist. Skipping seed.");
            return;
        }

        Console.WriteLine("[AdminSeeder] Seeding initial admin accounts...");

        var admins = new List<AdminAccount>
        {
            // Super Admin - READ ONLY access to audit logs
            new AdminAccount
            {
                Id = Guid.NewGuid(),
                Email = "superadmin@fooddelivery.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
                FullName = "Super Administrator",
                Role = AdminRole.SuperAdmin,
                RegionCode = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Master Admin - Final approval authority
            new AdminAccount
            {
                Id = Guid.NewGuid(),
                Email = "masteradmin@fooddelivery.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("MasterAdmin@123"),
                FullName = "Master Administrator",
                Role = AdminRole.AdminRestaurantMaster,
                RegionCode = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Region Admin - Hanoi
            new AdminAccount
            {
                Id = Guid.NewGuid(),
                Email = "admin.hn@fooddelivery.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("RegionAdmin@123"),
                FullName = "Region Admin - Hanoi",
                Role = AdminRole.AdminRestaurantRegion,
                RegionCode = "HN",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Region Admin - Ho Chi Minh City
            new AdminAccount
            {
                Id = Guid.NewGuid(),
                Email = "admin.sgn@fooddelivery.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("RegionAdmin@123"),
                FullName = "Region Admin - Sai Gon",
                Role = AdminRole.AdminRestaurantRegion,
                RegionCode = "SGN",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Region Admin - Da Nang
            new AdminAccount
            {
                Id = Guid.NewGuid(),
                Email = "admin.dn@fooddelivery.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("RegionAdmin@123"),
                FullName = "Region Admin - Da Nang",
                Role = AdminRole.AdminRestaurantRegion,
                RegionCode = "DN",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.AdminAccounts.AddRange(admins);
        await context.SaveChangesAsync();

        Console.WriteLine($"[AdminSeeder] Created {admins.Count} admin accounts:");
        foreach (var admin in admins)
        {
            Console.WriteLine($"  - {admin.Email} ({admin.Role}) Region: {admin.RegionCode ?? "Global"}");
        }
    }
}
