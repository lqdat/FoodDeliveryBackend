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
        // Log to file to debug
        void Log(string msg) => System.IO.File.AppendAllText("seed_log.txt", $"{DateTime.UtcNow}: {msg}\n");

        try 
        {
            Log("Starting Data Seeding...");
            Console.WriteLine("Starting Data Seeding...");
            var now = DateTime.UtcNow;

            // ---------------------------------------------------------
            // 1. Seed Users & Profiles
            // ---------------------------------------------------------
            
            // Define Roles (Implicitly)
            const int ROLE_ADMIN = 1;
            const int ROLE_CUSTOMER = 2;
            const int ROLE_MERCHANT = 3;
            const int ROLE_DRIVER = 4;

            // 1a. Admin User
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
            if (adminUser == null)
            {
                Console.WriteLine("Seeding Admin...");
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Admin",
                    PhoneNumber = "0900000001",
                    Email = "admin@example.com",
                    AvatarUrl = "https://i.pravatar.cc/150?u=admin",
                    PasswordHash = "dummy_hash_admin",
                    Role = ROLE_ADMIN,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(adminUser);
            }

            // 1b. Customer User
            var customerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@example.com");
            if (customerUser == null)
            {
                Console.WriteLine("Seeding Customer...");
                customerUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Nguyen Van Khach",
                    PhoneNumber = "0900000002",
                    Email = "customer@example.com",
                    AvatarUrl = "https://i.pravatar.cc/150?u=customer",
                    PasswordHash = "dummy_hash_customer",
                    Role = ROLE_CUSTOMER,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(customerUser);

                await context.Customers.AddAsync(new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = customerUser.Id,
                });
            }

            // 1c. Merchant User
            var merchantUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "merchant@example.com");
            if (merchantUser == null)
            {
                Console.WriteLine("Seeding Merchant User...");
                merchantUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Tran Van Chu Quan",
                    PhoneNumber = "0900000003",
                    Email = "merchant@example.com",
                    AvatarUrl = "https://i.pravatar.cc/150?u=merchant",
                    PasswordHash = "dummy_hash_merchant",
                    Role = ROLE_MERCHANT,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(merchantUser);

                await context.Merchants.AddAsync(new Merchant
                {
                    Id = Guid.NewGuid(),
                    UserId = merchantUser.Id,
                    BusinessName = "Official FoodDelivery Merchant",
                    IsApproved = true,
                    ApprovedAt = now,
                    CreatedAt = now,
                    IsDeleted = false
                });
            }

            // 1d. Driver User
            var driverUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "driver@example.com");
            if (driverUser == null)
            {
                Console.WriteLine("Seeding Driver...");
                driverUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Le Van Tai Xe",
                    PhoneNumber = "0900000004",
                    Email = "driver@example.com",
                    AvatarUrl = "https://i.pravatar.cc/150?u=driver",
                    PasswordHash = "dummy_hash_driver",
                    Role = ROLE_DRIVER,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(driverUser);

                await context.Drivers.AddAsync(new Driver
                {
                    Id = Guid.NewGuid(),
                    UserId = driverUser.Id,
                    VehicleType = "Bike",
                    VehicleBrand = "Honda",
                    VehiclePlate = "59-X1 123.45",
                    Status = 1, // Available
                    IsApproved = true,
                    CreatedAt = now
                });
            }

            // Ensure changes saved before linking
            await context.SaveChangesAsync();
            
            // Get the Merchant Profile ID for linking restaurants
            // Use the verified merchant user we just seeded/found
            var merchantProfile = await context.Merchants.FirstOrDefaultAsync(m => m.UserId == merchantUser.Id);

            // ---------------------------------------------------------
            // 3. Seed Categories
            // ---------------------------------------------------------
            var categoriesData = new List<(string Name, string Sec, string Icon, string Bg)>
            {
                ("Cơm", "Rice", "https://images.unsplash.com/photo-1596560548464-f010549b84d7", "#FFF0E6"),
                ("Bún/Phở", "Noodles", "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43", "#E6F4FF"),
                ("Trà Sữa", "Milk Tea", "https://images.unsplash.com/photo-1556679343-c7306c1976bc", "#FFF9E6"),
                ("Fast Food", "Fast Food", "https://images.unsplash.com/photo-1561758033-d89a9ad46330", "#FFEBEE"),
                ("Ăn vặt", "Snack", "https://images.unsplash.com/photo-1565557623262-b51c2513a641", "#E8F5E9"),
                ("Healthy", "Diet", "https://images.unsplash.com/photo-1512621776951-a57141f2eefd", "#E0F2F1")
            };

            // Using logic to fetch IDs of existing categories or create new ones
            var categoriesMap = new Dictionary<string, Guid>();

            foreach (var c in categoriesData)
            {
                var existingCat = await context.FoodCategories.FirstOrDefaultAsync(x => x.Name == c.Name);
                if (existingCat != null)
                {
                    categoriesMap[c.Name] = existingCat.Id;
                }
                else
                {
                    var newCat = new FoodCategory
                    {
                        Id = Guid.NewGuid(),
                        Name = c.Name,
                        NameSecondary = c.Sec,
                        IconUrl = c.Icon,
                        BackgroundColor = c.Bg,
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = now
                    };
                    await context.FoodCategories.AddAsync(newCat);
                    categoriesMap[c.Name] = newCat.Id;
                }
            }
            await context.SaveChangesAsync();


            // ---------------------------------------------------------
            // 4. Seed Restaurants
            // ---------------------------------------------------------
            // Check if we have restaurants for this merchant
            bool hasRestaurants = await context.Restaurants.AnyAsync(r => r.MerchantId == merchantProfile.Id);
            
            if (!hasRestaurants)
            {
                Console.WriteLine("Seeding Restaurants...");
                var restaurantsToAdd = new List<Restaurant>
                {
                    new Restaurant
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = merchantProfile.Id,
                        Name = "Cơm Tấm Sài Gòn",
                        Rating = 4.8, 
                        RatingCount = 1200, 
                        DeliveryTime = 20, 
                        MinPrice = 35000, 
                        DeliveryFee = 15000,
                        ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733",
                        Address = "123 Nguyễn Văn Cừ, Q.5",
                        CategoryId = categoriesMap["Cơm"],
                        CreatedAt = now,
                        IsApproved = true,
                        IsOpen = true,
                        Tags = new[] { "Cơm Tấm", "Sườn Nướng", "Ăn Trưa" },
                        Distance = 2.5
                    },
                    new Restaurant
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = merchantProfile.Id,
                        Name = "Phở Lý Quốc Sư",
                        Rating = 4.5, RatingCount = 500, DeliveryTime = 25, 
                        MinPrice = 50000, DeliveryFee = 18000,
                        ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43",
                        Address = "45 Võ Văn Tần, Q.3",
                        CategoryId = categoriesMap["Bún/Phở"],
                        CreatedAt = now,
                        IsApproved = true,
                        IsOpen = true,
                        Tags = new[] { "Phở", "Bún", "Ăn Sáng" },
                        Distance = 3.1
                    },
                    new Restaurant
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = merchantProfile.Id,
                        Name = "Tocotoco Bubble Tea",
                        Rating = 4.2, RatingCount = 300, DeliveryTime = 15, 
                        MinPrice = 30000, DeliveryFee = 12000,
                        ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc",
                        Address = "10 Hai Bà Trưng, Q.1",
                        CategoryId = categoriesMap["Trà Sữa"],
                        CreatedAt = now,
                        IsApproved = true,
                        IsOpen = true,
                        Tags = new[] { "Trà Sữa", "Trân Châu", "Giải Khát" },
                        Distance = 1.2
                    },
                    new Restaurant
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = merchantProfile.Id,
                        Name = "KFC - Gà Rán",
                        Rating = 4.6, RatingCount = 2000, DeliveryTime = 30, 
                        MinPrice = 40000, DeliveryFee = 20000,
                        ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330",
                        Address = "Lotte Mart, Q.7",
                        CategoryId = categoriesMap["Fast Food"],
                        CreatedAt = now,
                        IsApproved = true,
                        IsOpen = true,
                        Tags = new[] { "Gà Rán", "KFC", "Fast Food" },
                        Distance = 5.0
                    },
                    new Restaurant
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = merchantProfile.Id,
                        Name = "Bánh Tráng Trộn Cô Lan",
                        Rating = 4.9, RatingCount = 150, DeliveryTime = 15, 
                        MinPrice = 20000, DeliveryFee = 15000,
                        ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641",
                        Address = "Hẻm 51, Q.3",
                        CategoryId = categoriesMap["Ăn vặt"],
                        CreatedAt = now,
                        IsApproved = true,
                        IsOpen = true,
                        Tags = new[] { "Bánh Tráng", "Ăn Vặt", "Vỉa Hè" },
                        Distance = 2.0
                    },
                    new Restaurant
                    {
                        Id = Guid.NewGuid(),
                        MerchantId = merchantProfile.Id,
                        Name = "Salad & Smoothies",
                        Rating = 4.7, RatingCount = 80, DeliveryTime = 25, 
                        MinPrice = 60000, DeliveryFee = 15000,
                        ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd",
                        Address = "Thảo Điền, Q.2",
                        CategoryId = categoriesMap["Healthy"],
                        CreatedAt = now,
                        IsApproved = true,
                        IsOpen = true,
                        Tags = new[] { "Salad", "Healthy", "Giảm Cân" },
                        Distance = 6.5
                    }
                };
                await context.Restaurants.AddRangeAsync(restaurantsToAdd);
                await context.SaveChangesAsync();
                Console.WriteLine("Restaurants saved.");

                // 5. Menu Items (Only if we just added restaurants)
                Console.WriteLine("Seeding Menu Items...");
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
            else
            {
                Console.WriteLine("Restaurants already exist.");
            }

            // ---------------------------------------------------------
            // 6. Seed Orders (Sample History)
            // ---------------------------------------------------------
            bool hasOrders = await context.Orders.AnyAsync();
            if (!hasOrders && customerUser != null && merchantProfile != null) 
            {
                Console.WriteLine("Seeding Sample Orders...");
                
                // Get a restaurant
                var comTamRest = await context.Restaurants
                                    .Include(r => r.MenuItems)
                                    .FirstOrDefaultAsync(r => r.Name == "Cơm Tấm Sài Gòn");
                                    
                if (comTamRest != null && comTamRest.MenuItems.Any())
                {
                    var menuItem = comTamRest.MenuItems.First();
                    
                    // Order 1: Completed
                    var order1 = new Order
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "ORD-" + DateTime.UtcNow.Ticks,
                        CustomerId = (await context.Customers.FirstAsync(c => c.UserId == customerUser.Id)).Id,
                        RestaurantId = comTamRest.Id,
                        DeliveryAddress = "123 Le Loi, Q1",
                        Subtotal = menuItem.Price,
                        DeliveryFee = 15000,
                        TotalAmount = menuItem.Price + 15000,
                        Status = 5, // Completed
                        PaymentMethod = 1, // Cash
                        CreatedAt = now.AddDays(-1),
                        ConfirmedAt = now.AddDays(-1).AddMinutes(5),
                        PickedUpAt = now.AddDays(-1).AddMinutes(20),
                        DeliveredAt = now.AddDays(-1).AddMinutes(35)
                    };
                    
                    await context.Orders.AddAsync(order1);
                    
                    // Order Item
                    await context.OrderItems.AddAsync(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order1.Id,
                        MenuItemId = menuItem.Id,
                        Quantity = 1,
                        Price = menuItem.Price,
                        Name = menuItem.Name,
                        TotalPrice = menuItem.Price
                    });

                    // Order Tracking
                    await context.OrderTrackings.AddRangeAsync(new List<OrderTracking>
                    {
                        new OrderTracking { Id = Guid.NewGuid(), OrderId = order1.Id, Status = 0, Description = "Order Placed", CreatedAt = now.AddDays(-1) },
                        new OrderTracking { Id = Guid.NewGuid(), OrderId = order1.Id, Status = 1, Description = "Restaurant Accepted", CreatedAt = now.AddDays(-1).AddMinutes(5) },
                         new OrderTracking { Id = Guid.NewGuid(), OrderId = order1.Id, Status = 5, Description = "Delivered Successfully", CreatedAt = now.AddDays(-1).AddMinutes(35) }
                    });
                    
                    await context.SaveChangesAsync();
                }
            }
            
            Log("Seed completed successfully!");
            Console.WriteLine("Seed completed successfully!");
        }
        catch (Exception ex)
        {
             var msg = $"CRITICAL ERROR DURING SEEDING: {ex.Message} \n {ex.StackTrace}";
             Log(msg);
             Console.WriteLine(msg);
             if (ex.InnerException != null) {
                 Log($"Inner: {ex.InnerException.Message}");
                 Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
             }
        }
    }
}
