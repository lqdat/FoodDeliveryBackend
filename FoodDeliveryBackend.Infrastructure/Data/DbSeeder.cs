using System.Text.Json;
using FoodDeliveryBackend.Core.Entities;
using Microsoft.EntityFrameworkCore;

// Important: Ensure this namespace matches your clean architecture
namespace FoodDeliveryBackend.Infrastructure.Data;

public static class DbSeeder
{
    private static readonly string LogFile = "seed_log.txt";

    private static void Log(string message)
    {
        string logLine = $"{DateTime.UtcNow.AddHours(7)}: {message}{Environment.NewLine}";
        File.AppendAllText(LogFile, logLine);
    }

    public static async Task SeedAsync(FoodDeliveryDbContext context)
    {
        Log("Starting Data Seeding...");
        Console.WriteLine("Starting Data Seeding...");
        
        try 
        {
            var now = DateTime.UtcNow;

            // 1. Roles & Users
            // ---------------------------------------------------------
            // Ensure Roles (Implicitly handled by User.Role integer for this MVP)
            // 1 = Admin, 2 = Customer, 3 = Merchant, 4 = Driver
            
            // --- Admin ---
            var adminEmail = "admin@example.com";
            if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                await context.Users.AddAsync(new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Admin",
                    Email = adminEmail,
                    PhoneNumber = "0900000001",
                    PasswordHash = "hashed_password_admin", // In real app, use BCrypt
                    Role = 1,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            // --- Customer ---
            var customerEmail = "customer@example.com";
            var customerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);
            if (customerUser == null)
            {
                customerUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Nguyen Van Khach",
                    Email = customerEmail,
                    PhoneNumber = "0900000002",
                    PasswordHash = "hashed_password_customer",
                    Role = 2,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(customerUser);
            }
            
            // Ensure Customer Profile
            if (!await context.Customers.AnyAsync(c => c.UserId == customerUser.Id))
            {
                await context.Customers.AddAsync(new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = customerUser.Id,
                    LoyaltyPoints = 100,
                    IsActive = true,
                    CreatedAt = now
                });
            }

            // --- Merchant ---
            var merchantEmail = "merchant@example.com";
            var merchantUser = await context.Users.FirstOrDefaultAsync(u => u.Email == merchantEmail);
            if (merchantUser == null)
            {
                merchantUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "The Merchant Owner",
                    Email = merchantEmail,
                    PhoneNumber = "0900000003",
                    PasswordHash = "hashed_password_merchant",
                    Role = 3,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(merchantUser);
            }

            // Ensure Merchant Profile
            var merchantProfile = await context.Merchants.FirstOrDefaultAsync(m => m.UserId == merchantUser.Id);
            if (merchantProfile == null)
            {
                merchantProfile = new Merchant
                {
                    Id = Guid.NewGuid(),
                    UserId = merchantUser.Id,
                    BusinessName = "Delicious Foods Corp",
                    ContactEmail = merchantEmail,
                    ContactPhone = "0900000003",
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = now
                };
                await context.Merchants.AddAsync(merchantProfile);
            }

            // --- Driver ---
            var driverEmail = "driver@example.com";
            var driverUser = await context.Users.FirstOrDefaultAsync(u => u.Email == driverEmail);
            if (driverUser == null)
            {
                driverUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Nguyen Van Tai Xe",
                    Email = driverEmail,
                    PhoneNumber = "0900000004",
                    PasswordHash = "hashed_password_driver",
                    Role = 4,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(driverUser);
            }

            // Ensure Driver Profile
            if (!await context.Drivers.AnyAsync(d => d.UserId == driverUser.Id))
            {
                await context.Drivers.AddAsync(new Driver
                {
                    Id = Guid.NewGuid(),
                    UserId = driverUser.Id,
                    VehicleType = "Honda Vision",
                    LicensePlate = "59-X1 123.45",
                    IsOnline = false,
                    IsVerified = true,
                    Rating = 5.0,
                    WalletBalance = 0,
                    CreatedAt = now
                });
            }

            await context.SaveChangesAsync();

            // ---------------------------------------------------------
            // 2. Categories
            // ---------------------------------------------------------
            if (!await context.FoodCategories.AnyAsync())
            {
                var categories = new List<FoodCategory>
                {
                    new FoodCategory { Id = Guid.NewGuid(), Name = "Cơm", ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19" },
                    new FoodCategory { Id = Guid.NewGuid(), Name = "Bún/Phở", ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43" },
                    new FoodCategory { Id = Guid.NewGuid(), Name = "Đồ Ăn Nhanh", ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330" },
                    new FoodCategory { Id = Guid.NewGuid(), Name = "Trà Sữa", ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc" },
                    new FoodCategory { Id = Guid.NewGuid(), Name = "Ăn Vặt", ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641" },
                    new FoodCategory { Id = Guid.NewGuid(), Name = "Healthy", ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd" }
                };
                await context.FoodCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                
                Log($"Seeded {categories.Count} categories.");
            }

            // ---------------------------------------------------------
            // 3. Restaurants & Menu Items
            // ---------------------------------------------------------
            if (!await context.Restaurants.AnyAsync(r => r.MerchantId == merchantProfile.Id))
            {
                // Retrieve Categories for mapping
                var cats = await context.FoodCategories.ToListAsync();
                
                var restaurants = new List<Restaurant>
                {
                    new Restaurant 
                    { 
                        Id = Guid.NewGuid(), 
                        MerchantId = merchantProfile.Id,
                        Name = "Cơm Tấm Sài Gòn", 
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Cơm")?.Id, // Link to Cơm
                        ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733",
                        Address = "123 Nguyễn Văn Cừ, Q.5",
                        Rating = 4.8, RatingCount = 1200, DeliveryTime = 20, DeliveryFee = 15000, MinPrice = 35000, Distance = 2.5,
                        Tags = new[] { "Cơm Tấm", "Sườn Nướng", "Ăn Trưa" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    },
                    new Restaurant 
                    { 
                        Id = Guid.NewGuid(), 
                        MerchantId = merchantProfile.Id,
                        Name = "KFC - Gà Rán", 
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Đồ Ăn Nhanh")?.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330",
                        Address = "Lotte Mart, Q.7",
                        Rating = 4.6, RatingCount = 2000, DeliveryTime = 30, DeliveryFee = 20000, MinPrice = 40000, Distance = 5.0,
                         Tags = new[] { "Gà Rán", "KFC", "Fast Food" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    },
                     new Restaurant 
                    { 
                        Id = Guid.NewGuid(), 
                        MerchantId = merchantProfile.Id,
                        Name = "Trà Sữa Koi Thé", 
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Trà Sữa")?.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc",
                        Address = "Vivo City, Q.7",
                        Rating = 4.9, RatingCount = 500, DeliveryTime = 15, DeliveryFee = 10000, MinPrice = 30000, Distance = 1.0,
                         Tags = new[] { "Trà Sữa", "Macchiato", "Trân Châu" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    }
                };

                await context.Restaurants.AddRangeAsync(restaurants);
                await context.SaveChangesAsync();

                // 4. Menus
                foreach (var rest in restaurants)
                {
                    // Create Menu Category
                    var menuCat = new MenuCategory 
                    { 
                        Id = Guid.NewGuid(), 
                        RestaurantId = rest.Id, 
                        Name = "Món Chính", 
                        DisplayOrder = 1 
                    };
                    await context.MenuCategories.AddAsync(menuCat);
                    
                    // Create Menu Items
                    var items = new List<MenuItem>();
                    if (rest.Name.Contains("Cơm"))
                    {
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = menuCat.Id, Name = "Cơm Sườn", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Cơm sườn nướng than hồng" });
                         items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = menuCat.Id, Name = "Cơm Bì Chả", Price = 40000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Cơm bì chả truyền thống" });
                    }
                    else if (rest.Name.Contains("KFC"))
                    {
                         items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = menuCat.Id, Name = "Combo Gà Rán", Price = 89000, ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330", Description = "2 miếng gà + khoai + nước" });
                    }
                     else if (rest.Name.Contains("Trà Sữa"))
                    {
                         items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = menuCat.Id, Name = "Hồng Trà Macchiato", Price = 35000, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc", Description = "Size M" });
                    }
                    
                    await context.MenuItems.AddRangeAsync(items);
                }
                
                await context.SaveChangesAsync();
                Log($"Seeded {restaurants.Count} restaurants with menus.");
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
                // FIX: Use MenuCategories to access items
                var comTamRest = await context.Restaurants
                                    .Include(r => r.MenuCategories)
                                    .ThenInclude(mc => mc.MenuItems)
                                    .FirstOrDefaultAsync(r => r.Name == "Cơm Tấm Sài Gòn");
                                    
                if (comTamRest != null && comTamRest.MenuCategories.Any())
                {
                    // FIX: Select from MenuCategories
                    var menuItem = comTamRest.MenuCategories.SelectMany(mc => mc.MenuItems).FirstOrDefault();
                    if (menuItem != null)
                    {
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
                            // FIX: Use correctly mapped properties
                            UnitPrice = menuItem.Price,
                            ItemName = menuItem.Name, // Explicitly verified: ItemName property exists in OrderItem
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
            }
            
            // ---------------------------------------------------------
            // 7. Seed Vouchers
            // ---------------------------------------------------------
            if (!await context.Vouchers.AnyAsync())
            {
                var vouchers = new List<Voucher>
                {
                    new Voucher
                    {
                        Id = Guid.NewGuid(),
                        Code = "WELCOME50",
                        Name = "Giảm 50% Bạn Mới",
                        Description = "Giảm tối đa 50k cho đơn đầu tiên",
                        Type = 1, // Percentage
                        DiscountValue = 50,
                        MaxDiscountAmount = 50000,
                        MinOrderAmount = 100000,
                        StartDate = now.AddDays(-1),
                        EndDate = now.AddDays(30),
                        MaxUsage = 1000,
                        UsedCount = 0,
                        IsActive = true,
                        IconUrl = "https://cdn-icons-png.flaticon.com/512/726/726496.png",
                        CreatedAt = now
                    },
                    new Voucher
                    {
                         Id = Guid.NewGuid(),
                        Code = "GIAM20K",
                        Name = "Giảm 20k Đơn 100k",
                        Description = "Ưu đãi cho mọi đơn hàng",
                        Type = 0, // Fixed
                        DiscountValue = 20000,
                        MaxDiscountAmount = 20000,
                        MinOrderAmount = 100000,
                        StartDate = now.AddDays(-1),
                        EndDate = now.AddDays(15),
                        IsActive = true,
                         IconUrl = "https://cdn-icons-png.flaticon.com/512/879/879757.png",
                        CreatedAt = now
                    }
                };
                
                await context.Vouchers.AddRangeAsync(vouchers);
                await context.SaveChangesAsync();
                Log("Seeded sample Vouchers.");
            }

            Log("Seed completed successfully!");
            Console.WriteLine("Seed completed successfully!");
        }
        catch (Exception ex)
        {
            Log($"Error seeding data: {ex.Message} {ex.StackTrace}");
            Console.WriteLine($"Error seeding data: {ex.Message}");
            throw; // Rethrow to ensure startup fails? Or log and continue.
        }
    }
}
