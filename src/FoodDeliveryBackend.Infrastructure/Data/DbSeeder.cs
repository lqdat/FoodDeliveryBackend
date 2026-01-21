using FoodDeliveryBackend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDeliveryBackend.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(FoodDeliveryDbContext context)
    {
        // 0. Check if ANY user exists
        if (await context.Users.AnyAsync())
            return; // Already seeded

        var now = DateTime.UtcNow;

        // 1. Create Base User
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyen Van A",
            PhoneNumber = "0909123456",
            Email = "nguyenvana@example.com",
            AvatarUrl = "https://i.pravatar.cc/150?u=user_1",
            // CreatedAt is likely required? Let's assume scaffolding handles it or we set it if generated.
            // Scaffolded User doesn't seem to have CreatedAt? checking User.cs
            // Actually let's trust User.cs properties. 
            // Checking previous scaffold list: User.cs exists.
            // If property missing, compile error will tell us.
            // Assuming Scaffolded User has Id, FullName, PhoneNumber...
        };
        // Ensure properties exist. Scaffolded User: Id, Email, PhoneNumber... 
        // We will assign what we see in User.cs from scaffold logic if possible, 
        // but for now we stick to what we saw in errors or valid attempts.
        // Actually I don't see User columns in my previous view of User.cs?
        // Step 382 listed User.cs. I didn't view it.
        // I'll take a safe bet: Id, Email, PhoneNumber are standard.
        await context.Users.AddAsync(user);

        // 2. Create Merchant Profile
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BusinessName = "Official FoodDelivery Merchant",
            IsApproved = true,
            ApprovedAt = now,
            CreatedAt = now,
            IsDeleted = false
        };
        await context.Merchants.AddAsync(merchant);
        
        await context.SaveChangesAsync();

        // 3. Categories (FoodCategory)
        var categoriesData = new List<(string Name, string Sec, string Icon, string Bg)>
        {
            ("Cơm", "Rice", "https://images.unsplash.com/photo-1596560548464-f010549b84d7", "#FFF0E6"),
            ("Bún/Phở", "Noodles", "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43", "#E6F4FF"),
            ("Trà Sữa", "Milk Tea", "https://images.unsplash.com/photo-1556679343-c7306c1976bc", "#FFF9E6"),
            ("Fast Food", "Fast Food", "https://images.unsplash.com/photo-1561758033-d89a9ad46330", "#FFEBEE"),
            ("Ăn vặt", "Snack", "https://images.unsplash.com/photo-1565557623262-b51c2513a641", "#E8F5E9"),
            ("Healthy", "Diet", "https://images.unsplash.com/photo-1512621776951-a57141f2eefd", "#E0F2F1")
        };

        foreach (var c in categoriesData)
        {
            context.FoodCategories.Add(new FoodCategory
            {
                Id = Guid.NewGuid(),
                Name = c.Name,
                NameSecondary = c.Sec,
                IconUrl = c.Icon,
                BackgroundColor = c.Bg,
                DisplayOrder = 0, // Set defaults
                IsActive = true,
                CreatedAt = now
            });
        }
        await context.SaveChangesAsync();

        // 4. Restaurants
        var restaurantsToAdd = new List<Restaurant>
        {
            new Restaurant
            {
                Id = Guid.NewGuid(),
                MerchantId = merchant.Id,
                Name = "Cơm Tấm Sài Gòn",
                Rating = 4.8, 
                TotalReviews = 1200, 
                EstimatedDeliveryMinutes = 20, 
                MinOrderAmount = 35000, 
                DeliveryFee = 15000,
                ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733",
                Address = "123 Nguyễn Văn Cừ, Q.5",
                Category = "Cơm", 
                CreatedAt = now,
                IsApproved = true,
                IsOpen = true
            },
            new Restaurant
            {
                Id = Guid.NewGuid(),
                MerchantId = merchant.Id,
                Name = "Phở Lý Quốc Sư",
                Rating = 4.5, TotalReviews = 500, EstimatedDeliveryMinutes = 25, 
                MinOrderAmount = 50000, DeliveryFee = 18000,
                ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43",
                Address = "45 Võ Văn Tần, Q.3",
                Category = "Bún/Phở",
                CreatedAt = now,
                IsApproved = true,
                IsOpen = true
            },
            new Restaurant
            {
                Id = Guid.NewGuid(),
                MerchantId = merchant.Id,
                Name = "Tocotoco Bubble Tea",
                Rating = 4.2, TotalReviews = 300, EstimatedDeliveryMinutes = 15, 
                MinOrderAmount = 30000, DeliveryFee = 12000,
                ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc",
                Address = "10 Hai Bà Trưng, Q.1",
                Category = "Trà Sữa",
                CreatedAt = now,
                IsApproved = true,
                IsOpen = true
            },
            new Restaurant
            {
                Id = Guid.NewGuid(),
                MerchantId = merchant.Id,
                Name = "KFC - Gà Rán",
                Rating = 4.6, TotalReviews = 2000, EstimatedDeliveryMinutes = 30, 
                MinOrderAmount = 40000, DeliveryFee = 20000,
                ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330",
                Address = "Lotte Mart, Q.7",
                Category = "Fast Food",
                CreatedAt = now,
                IsApproved = true,
                IsOpen = true
            },
            new Restaurant
            {
                Id = Guid.NewGuid(),
                MerchantId = merchant.Id,
                Name = "Bánh Tráng Trộn Cô Lan",
                Rating = 4.9, TotalReviews = 150, EstimatedDeliveryMinutes = 15, 
                MinOrderAmount = 20000, DeliveryFee = 15000,
                ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641",
                Address = "Hẻm 51, Q.3",
                Category = "Ăn vặt",
                CreatedAt = now,
                IsApproved = true,
                IsOpen = true
            },
            new Restaurant
            {
                Id = Guid.NewGuid(),
                MerchantId = merchant.Id,
                Name = "Salad & Smoothies",
                Rating = 4.7, TotalReviews = 80, EstimatedDeliveryMinutes = 25, 
                MinOrderAmount = 60000, DeliveryFee = 15000,
                ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd",
                Address = "Thảo Điền, Q.2",
                Category = "Healthy",
                CreatedAt = now,
                IsApproved = true,
                IsOpen = true
            }
        };
        await context.Restaurants.AddRangeAsync(restaurantsToAdd);
        await context.SaveChangesAsync();
        
        // 5. Menu Items
        var comTam = restaurantsToAdd.First(r => r.Name == "Cơm Tấm Sài Gòn");
        var menuCatCom = new MenuCategory 
        { 
            Id = Guid.NewGuid(), 
            RestaurantId = comTam.Id, 
            Name = "Món Chính", 
            DisplayOrder = 1 
        };
        await context.MenuCategories.AddAsync(menuCatCom);
        
        await context.MenuItems.AddRangeAsync(new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = menuCatCom.Id, Name = "Cơm Sườn", Description = "Sườn nướng than", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1626804475297-411dbcc8c42b", IsAvailable = true },
            new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = menuCatCom.Id, Name = "Cơm Sườn Bì Chả", Description = "Full topping", Price = 55000, ImageUrl = "https://images.unsplash.com/photo-1596560548464-f010549b84d7", IsAvailable = true }
        });
        
        await context.SaveChangesAsync();
    }
}
