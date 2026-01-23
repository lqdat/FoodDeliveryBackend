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
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Admin",
                    Email = adminEmail,
                    PhoneNumber = "0900000001",
                    PasswordHash = "admin", 
                    Role = 1,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(adminUser);
            }
            else
            {
                adminUser.PasswordHash = "admin";
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
                    PasswordHash = "customer",
                    Role = 2,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(customerUser);
            }
            else
            {
                customerUser.PasswordHash = "customer";
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
                    PasswordHash = "merchant",
                    Role = 3,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(merchantUser);
            }
            else
            {
                merchantUser.PasswordHash = "merchant";
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
                    PasswordHash = "driver",
                    Role = 4,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await context.Users.AddAsync(driverUser);
            }
            else
            {
                driverUser.PasswordHash = "driver";
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
            // ---------------------------------------------------------
            // 2. Categories (Upsert: Update existing or Create new)
            // ---------------------------------------------------------
            var categoryDefinitions = new List<FoodCategory>
            {
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Cơm", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1531/1531338.png", 
                    BackgroundColor = "#FFF5E6", // Light Orange
                    ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Bún/Phở", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/3421/3421683.png", 
                    BackgroundColor = "#E6F7FF", // Light Blue
                    ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Trà Sữa", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/3081/3081162.png", 
                    BackgroundColor = "#FFF0F6", // Light Pink
                    ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Ăn Vặt", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/737/737967.png", 
                    BackgroundColor = "#FFFFE6", // Light Yellow
                    ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Healthy", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2913/2913456.png", 
                    BackgroundColor = "#E6FFFA", // Light Green
                    ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Deal Hời", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/726/726496.png", 
                    BackgroundColor = "#FFF1F0", // Light Red
                    ImageUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Đi Chợ", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1261/1261163.png", 
                    BackgroundColor = "#F9F0FF", // Light Purple
                    ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Thêm", 
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2948/2948032.png", 
                    BackgroundColor = "#F5F5F5", // Light Grey
                    ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3" 
                }
            };

            foreach (var catDef in categoryDefinitions)
            {
                var existing = await context.FoodCategories.FirstOrDefaultAsync(c => c.Name == catDef.Name);
                if (existing == null)
                {
                    await context.FoodCategories.AddAsync(catDef);
                }
                else
                {
                    existing.IconUrl = catDef.IconUrl;
                    existing.BackgroundColor = catDef.BackgroundColor;
                    existing.ImageUrl = catDef.ImageUrl;
                }
            }
            await context.SaveChangesAsync();
            await context.SaveChangesAsync();
            Log($"Seeded/Updated {categoryDefinitions.Count} categories with Icons and Colors.");

            // Self-Correct: Remove duplicates if any (Safety check)
            var duplicateCats = await context.FoodCategories
                .GroupBy(c => c.Name)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Skip(1)) // Keep one, select the rest
                .ToListAsync();

            if (duplicateCats.Any())
            {
                context.FoodCategories.RemoveRange(duplicateCats);
                await context.SaveChangesAsync();
                Log($"Removed {duplicateCats.Count} duplicate categories.");
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
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Ăn Vặt")?.Id, // Fast Food mapped to Ăn Vặt or we can add separate
                        ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec",
                        Address = "Lotte Mart, Q.7",
                        Rating = 4.6, RatingCount = 2000, DeliveryTime = 30, DeliveryFee = 20000, MinPrice = 40000, Distance = 5.0,
                         Tags = new[] { "Gà Rán", "KFC", "Fast Food" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    },
                     new Restaurant 
                    { 
                        Id = Guid.NewGuid(), 
                        MerchantId = merchantProfile.Id,
                        Name = "Koí Thé", 
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Trà Sữa")?.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4",
                        Address = "Vivo City, Q.7",
                        Rating = 4.9, RatingCount = 500, DeliveryTime = 15, DeliveryFee = 10000, MinPrice = 30000, Distance = 1.0,
                         Tags = new[] { "Trà Sữa", "Macchiato", "Trân Châu" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    },
                    new Restaurant 
                    { 
                        Id = Guid.NewGuid(), 
                        MerchantId = merchantProfile.Id,
                        Name = "Pizza Hut", 
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Ăn Vặt")?.Id, // Pizza mapped to Ăn Vặt
                        ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38",
                        Address = "345 Nguyễn Thị Thập, Q.7",
                        Rating = 4.5, RatingCount = 850, DeliveryTime = 40, DeliveryFee = 25000, MinPrice = 100000, Distance = 3.2,
                         Tags = new[] { "Pizza", "Mỳ Ý", "Fast Food" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    },
                     new Restaurant 
                    { 
                        Id = Guid.NewGuid(), 
                        MerchantId = merchantProfile.Id,
                        Name = "Highlands Coffee", 
                        CategoryId = cats.FirstOrDefault(c => c.Name == "Trà Sữa")?.Id, // Using Milk Tea/Drinks category
                        ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3",
                        Address = "Crescent Mall, Q.7",
                        Rating = 4.7, RatingCount = 1500, DeliveryTime = 25, DeliveryFee = 15000, MinPrice = 29000, Distance = 1.5,
                         Tags = new[] { "Cà Phê", "Trà", "Bánh" },
                        CreatedAt = now, IsApproved = true, IsOpen = true
                    }
                };

                await context.Restaurants.AddRangeAsync(restaurants);
                await context.SaveChangesAsync();

                // 4. Menus
                foreach (var rest in restaurants)
                {
                    // Define categories for each restaurant
                    var categories = new List<MenuCategory>();
                    
                    // Create common categories or specific ones
                    var mainCat = new MenuCategory { Id = Guid.NewGuid(), RestaurantId = rest.Id, Name = "Món Chính", DisplayOrder = 1 };
                    var sideCat = new MenuCategory { Id = Guid.NewGuid(), RestaurantId = rest.Id, Name = "Món Phụ", DisplayOrder = 2 };
                    var drinkCat = new MenuCategory { Id = Guid.NewGuid(), RestaurantId = rest.Id, Name = "Đồ Uống", DisplayOrder = 3 };

                    categories.Add(mainCat);
                    categories.Add(sideCat);
                    categories.Add(drinkCat);

                    await context.MenuCategories.AddRangeAsync(categories);
                    
                    // Create Menu Items
                    var items = new List<MenuItem>();
                    
                    if (rest.Name.Contains("Cơm"))
                    {
                        // Main
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Sườn", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Cơm sườn nướng than hồng" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Bì Chả", Price = 40000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Cơm bì chả truyền thống" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Gà Xối Mỡ", Price = 42000, ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43", Description = "Gà chiên giòn rụm" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Ba Rọi Nướng", Price = 48000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Ba rọi nướng đậm đà" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Sườn Non Kho", Price = 50000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Sườn non kho tộ" });
                        // Side
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Canh Khổ Qua", Price = 15000, ImageUrl = "https://images.unsplash.com/photo-1606850780554-b55ea2faa7b9", Description = "Canh khổ qua nhồi thịt" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Trứng Ốp La", Price = 5000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Trứng gà ta" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Lạp Xưởng", Price = 10000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "1 cây lạp xưởng tươi" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Canh Rong Biển", Price = 12000, ImageUrl = "https://images.unsplash.com/photo-1606850780554-b55ea2faa7b9", Description = "Canh rong biển thịt bằm" });
                        // Drink
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Trà Đá", Price = 2000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Mát lạnh" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Nước Sâm", Price = 10000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Sâm lạnh nhà nấu" });
                    }
                    else if (rest.Name.Contains("KFC"))
                    {
                        // Main
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Combo Gà Rán A", Price = 89000, ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec", Description = "2 Gà + 1 Khoai + 1 Pepsi" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Combo Gà Rán B", Price = 159000, ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec", Description = "4 Gà + 2 Khoai + 2 Pepsi" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Burger Tôm", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd", Description = "Burger tôm giòn tan" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Burger Zinger", Price = 59000, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd", Description = "Burger gà cay trứ danh" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Gà Quay", Price = 55000, ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19", Description = "Cơm gà quay sốt tiêu" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Mỳ Ý Gà Viên", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141", Description = "Mỳ ý sốt gà viên" });
                        // Side
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Khoai Tây Chiên (Vừa)", Price = 25000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Giòn rụm" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Khoai Tây Chiên (Lớn)", Price = 35000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Size lớn chia sẻ" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Gà Popcorn", Price = 39000, ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec", Description = "Gà viên vui miệng" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Salad Bắp Cải", Price = 15000, ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd", Description = "Coleslaw tươi mát" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Khoai Tây Nghiền", Price = 19000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Khoai tây nghiền sốt nâu" });
                        // Drink
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Pepsi Tươi", Price = 15000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Ly vừa" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "7Up", Price = 15000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Ly vừa" });
                    }
                    else if (rest.Name.Contains("Koí"))
                    {
                        // Main
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Hồng Trà Macchiato", Price = 35000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Size M - Lớp kem béo ngậy" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Lục Trà Trân Châu", Price = 40000, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc", Description = "Thơm ngon đậm vị" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Sữa Tươi Trân Châu", Price = 55000, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc", Description = "Đường đen Tiger" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Oolong Macchiato", Price = 42000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Trà Oolong đậm đà" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Trà Xanh Chanh Dây", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Chua ngọt sảng khoái" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Matcha Latte", Price = 52000, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc", Description = "Matcha Nhật Bản" });
                        // Side
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Trân Châu Hoàng Kim", Price = 10000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Dai ngon" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Thạch Dừa", Price = 8000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Giòn giòn" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Lô Hội", Price = 8000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Tươi mát" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Konjac Jelly", Price = 12000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Thạch dẻo" });
                        // Drink (More types)
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Trà Đào", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Có miếng đào tươi" });
                    }
                    else if (rest.Name.Contains("Pizza"))
                    {
                        // Main
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Hải Sản (M)", Price = 159000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Tôm, mực, thanh cua" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Hải Sản (L)", Price = 239000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Size Lớn" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Mỳ Ý Bò Bằm", Price = 89000, ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141", Description = "Sốt bò bằm cà chua" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Pepperoni", Price = 139000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Xúc xích Ý cay nhẹ" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Phô Mai Cao Cấp", Price = 149000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "3 loại phô mai" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Rau Củ", Price = 119000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Dành cho người ăn chay" });
                        // Side
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Salad Cá Ngừ", Price = 59000, ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd", Description = "Rau tươi sốt mayo" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Bánh Mì Bơ Tỏi", Price = 39000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Thơm lừng" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Khoai Tây Cười", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Vui nhộn cho bé" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Mực Chiên Giòn", Price = 79000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Mực vòng chiên bột" });
                        // Drink
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Coca Cola", Price = 20000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Chai 390ml" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Sprite", Price = 20000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Chai 390ml" });
                    }
                    else if (rest.Name.Contains("Highlands"))
                    {
                        // Main
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Phin Sữa Đá", Price = 29000, ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3", Description = "Cà phê phin truyền thống" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Phin Đen Đá", Price = 29000, ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3", Description = "Đậm đà tỉnh táo" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Trà Sen Vàng", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1595981267035-7b04ca84a82d", Description = "Trà sen kem sữa" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Freeze Trà Xanh", Price = 55000, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc", Description = "Đá xay mát lạnh" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Trà Thạch Đào", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Thanh mát giải nhiệt" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Phindi Hạnh Nhân", Price = 42000, ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3", Description = "Hương hạnh nhân thơm béo" });
                        // Side
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Bánh Mì Thịt Nướng", Price = 19000, ImageUrl = "https://images.unsplash.com/photo-1549449234-58d0092c6cc1", Description = "Bánh mì Việt Nam" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Bánh Mì Xíu Mại", Price = 19000, ImageUrl = "https://images.unsplash.com/photo-1549449234-58d0092c6cc1", Description = "Xíu mại sốt cà" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Mousse Đào", Price = 35000, ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19", Description = "Bánh ngọt tráng miệng" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Bánh Chuối", Price = 25000, ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19", Description = "Bánh chuối nướng" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Phô Mai Cà Phê", Price = 29000, ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19", Description = "Bánh phô mai vị cafe" });
                        // Drink
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Bạc Xỉu", Price = 29000, ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3", Description = "Nhiều sữa ít cafe" });
                        items.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Sữa Tươi", Price = 25000, ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3", Description = "Sữa tươi Vinamilk" });
                    }
                    
                    await context.MenuItems.AddRangeAsync(items);
                }
                
                await context.SaveChangesAsync();
                Log($"Seeded {restaurants.Count} restaurants with menus.");
            }
            else
            {
                Console.WriteLine("Restaurants already exist. Checking for Menu updates...");
                
                // Enrichment Logic for existing restaurants
                var cats = await context.FoodCategories.ToListAsync();
                var dbRestaurants = await context.Restaurants
                                        .Include(r => r.MenuCategories)
                                        .ThenInclude(mc => mc.MenuItems)
                                        .Where(r => r.MerchantId == merchantProfile.Id)
                                        .ToListAsync();

                foreach (var rest in dbRestaurants)
                {
                     // 1. Ensure Categories
                     var mainCat = rest.MenuCategories.FirstOrDefault(c => c.Name == "Món Chính");
                     if (mainCat == null) { mainCat = new MenuCategory { Id = Guid.NewGuid(), RestaurantId = rest.Id, Name = "Món Chính", DisplayOrder = 1 }; context.MenuCategories.Add(mainCat); }
                     
                     var sideCat = rest.MenuCategories.FirstOrDefault(c => c.Name == "Món Phụ");
                     if (sideCat == null) { sideCat = new MenuCategory { Id = Guid.NewGuid(), RestaurantId = rest.Id, Name = "Món Phụ", DisplayOrder = 2 }; context.MenuCategories.Add(sideCat); }
                     
                     var drinkCat = rest.MenuCategories.FirstOrDefault(c => c.Name == "Đồ Uống");
                     if (drinkCat == null) { drinkCat = new MenuCategory { Id = Guid.NewGuid(), RestaurantId = rest.Id, Name = "Đồ Uống", DisplayOrder = 3 }; context.MenuCategories.Add(drinkCat); }

                     // 2. Add New Items (Check by Name)
                     var potentialItems = new List<MenuItem>();

                    if (rest.Name.Contains("Cơm"))
                    {
                        // Main
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Sườn", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Cơm sườn nướng than hồng" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Bì Chả", Price = 40000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Cơm bì chả truyền thống" });
                        // New Items
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Ba Rọi Nướng", Price = 48000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Ba rọi nướng đậm đà" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Cơm Sườn Non Kho", Price = 50000, ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733", Description = "Sườn non kho tộ" });
                        // Side
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Canh Khổ Qua", Price = 15000, ImageUrl = "https://images.unsplash.com/photo-1606850780554-b55ea2faa7b9", Description = "Canh khổ qua nhồi thịt" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Lạp Xưởng", Price = 10000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "1 cây lạp xưởng tươi" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Canh Rong Biển", Price = 12000, ImageUrl = "https://images.unsplash.com/photo-1606850780554-b55ea2faa7b9", Description = "Canh rong biển thịt bằm" });
                        // Drink
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Nước Sâm", Price = 10000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Sâm lạnh nhà nấu" });
                    }
                    else if (rest.Name.Contains("KFC"))
                    {
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Burger Zinger", Price = 59000, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd", Description = "Burger gà cay trứ danh" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Mỳ Ý Gà Viên", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141", Description = "Mỳ ý sốt gà viên" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Salad Bắp Cải", Price = 15000, ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd", Description = "Coleslaw tươi mát" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Khoai Tây Nghiền", Price = 19000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Khoai tây nghiền sốt nâu" });
                    }
                    else if (rest.Name.Contains("Koí"))
                    {
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Oolong Macchiato", Price = 42000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Trà Oolong đậm đà" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Matcha Latte", Price = 52000, ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc", Description = "Matcha Nhật Bản" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Thạch Dừa", Price = 8000, ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4", Description = "Giòn giòn" });
                    }
                    else if (rest.Name.Contains("Pizza"))
                    {
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Phô Mai Cao Cấp", Price = 149000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "3 loại phô mai" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Pizza Rau Củ", Price = 119000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Dành cho người ăn chay" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Khoai Tây Cười", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38", Description = "Vui nhộn cho bé" });
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = drinkCat.Id, Name = "Sprite", Price = 20000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Chai 390ml" });
                    }
                    else if (rest.Name.Contains("Highlands"))
                    {
                        potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Phin Đen Đá", Price = 29000, ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3", Description = "Đậm đà tỉnh táo" });
                         potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = mainCat.Id, Name = "Trà Thạch Đào", Price = 45000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e", Description = "Thanh mát giải nhiệt" });
                         potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Bánh Mì Xíu Mại", Price = 19000, ImageUrl = "https://images.unsplash.com/photo-1549449234-58d0092c6cc1", Description = "Xíu mại sốt cà" });
                         potentialItems.Add(new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = sideCat.Id, Name = "Bánh Chuối", Price = 25000, ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19", Description = "Bánh chuối nướng" });
                    }

                    // Add items if not exists
                    foreach (var item in potentialItems)
                    {
                        if (!rest.MenuCategories.SelectMany(mc => mc.MenuItems).Any(i => i.Name == item.Name))
                        {
                            context.MenuItems.Add(item);
                        }
                    }
                }
                
                await context.SaveChangesAsync();
                Log("Enriched menus for existing restaurants.");
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

                    // Order 2: Active (Delivering)
                    var order2 = new Order
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "ORD-" + DateTime.UtcNow.Ticks + "-2",
                        CustomerId = (await context.Customers.FirstAsync(c => c.UserId == customerUser.Id)).Id,
                        RestaurantId = comTamRest.Id,
                        DeliveryAddress = "456 Nguyen Trai, Q5",
                        DeliveryLatitude = 10.755, 
                        DeliveryLongitude = 106.67,
                        Subtotal = menuItem.Price * 2,
                        DeliveryFee = 15000,
                        TotalAmount = (menuItem.Price * 2) + 15000,
                        Status = 4, // Delivering
                        PaymentMethod = 1, // Cash
                        EstimatedDeliveryMinutes = 15,
                        Distance = 3.5,
                        CreatedAt = now.AddMinutes(-20),
                        ConfirmedAt = now.AddMinutes(-15),
                        PreparedAt = now.AddMinutes(-10),
                        PickedUpAt = now.AddMinutes(-5)
                    };
                    
                    await context.Orders.AddAsync(order2);
                    
                    await context.OrderItems.AddAsync(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order2.Id,
                        MenuItemId = menuItem.Id,
                        Quantity = 2,
                        UnitPrice = menuItem.Price,
                        ItemName = menuItem.Name,
                        TotalPrice = menuItem.Price * 2,
                        CreatedAt = now.AddMinutes(-20)
                    });

                    await context.OrderTrackings.AddRangeAsync(new List<OrderTracking>
                    {
                        new OrderTracking { Id = Guid.NewGuid(), OrderId = order2.Id, Status = 0, Description = "Order Placed", CreatedAt = now.AddMinutes(-20) },
                        new OrderTracking { Id = Guid.NewGuid(), OrderId = order2.Id, Status = 4, Description = "Driver is on the way", CreatedAt = now.AddMinutes(-5) }
                    });

                    await context.SaveChangesAsync();
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
            
            // ---------------------------------------------------------
            // 8. Seed Notifications (New)
            // ---------------------------------------------------------
            if (!await context.Notifications.AnyAsync(n => n.UserId == customerUser.Id))
            {
                var notifs = new List<Notification>
                {
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "Chào mừng bạn mới!",
                        Message = "Nhập mã WELCOME50 để được giảm 50% cho đơn hàng đầu tiên.",
                        Type = 2, // Promo
                        IsRead = false,
                        CreatedAt = now.AddDays(-1)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "Đơn hàng đã giao thành công",
                        Message = "Đơn hàng Cơm Tấm Sài Gòn của bạn đã được giao. Chúc bạn ngon miệng!",
                        Type = 1, // Order
                        IsRead = true,
                        ReadAt = now.AddMinutes(-30),
                        CreatedAt = now.AddMinutes(-35)
                    },
                     new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "Tài xế đang đến",
                        Message = "Tài xế Nguyễn Văn Tài Xe đang trên đường giao đơn hàng mới nhất đến bạn.",
                        Type = 1, // Order
                        IsRead = false,
                        CreatedAt = now.AddMinutes(-5)
                    }
                };
                
                await context.Notifications.AddRangeAsync(notifs);
                await context.SaveChangesAsync();
                Log("Seeded sample Notifications.");
            }

            // ---------------------------------------------------------
            // 9. Seed Chat Messages (New)
            // ---------------------------------------------------------
            // Find the active order (Order 2 from step 6)
            // Find the active order (Order 2 from step 6)
            var customerId = (await context.Customers.FirstAsync(c => c.UserId == customerUser.Id)).Id;
            var activeOrder = await context.Orders.OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync(o => o.Status == 4 && o.CustomerId == customerId);
            
            if (activeOrder != null && !await context.ChatMessages.AnyAsync(m => m.OrderId == activeOrder.Id))
            {
                var chatMessages = new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        Id = Guid.NewGuid(),
                        OrderId = activeOrder.Id,
                        SenderId = driverUser.Id, // Driver
                        IsFromCustomer = false,
                        Content = "Chào bạn, tôi đã nhận đơn và đang đến quán.",
                        CreatedAt = activeOrder.CreatedAt.AddMinutes(5)
                    },
                    new ChatMessage
                    {
                         Id = Guid.NewGuid(),
                        OrderId = activeOrder.Id,
                        SenderId = customerUser.Id, // Customer
                        IsFromCustomer = true,
                        Content = "Dạ vâng, cảm ơn anh.",
                        CreatedAt = activeOrder.CreatedAt.AddMinutes(6)
                    },
                     new ChatMessage
                    {
                         Id = Guid.NewGuid(),
                        OrderId = activeOrder.Id,
                        SenderId = driverUser.Id, // Driver
                        IsFromCustomer = false,
                        Content = "Tôi đã lấy được món, khoảng 10 phút nữa tôi tới nơi nhé.",
                        CreatedAt = activeOrder.CreatedAt.AddMinutes(15)
                    }
                };

                await context.ChatMessages.AddRangeAsync(chatMessages);
                await context.SaveChangesAsync();
                Log("Seeded sample Chat Messages.");
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
