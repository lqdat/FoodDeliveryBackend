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

            // 0. RESET DATA (Requested by User)
            // ---------------------------------------------------------
            Log("CLEANING DATABASE FOR RESET...");
            Console.WriteLine("Cleaning database for reset...");
            
            // Clear in order of dependence
            context.OrderTrackings.RemoveRange(context.OrderTrackings);
            context.OrderItems.RemoveRange(context.OrderItems);
            context.Reviews.RemoveRange(context.Reviews);
            context.ChatMessages.RemoveRange(context.ChatMessages);
            context.DriverEarnings.RemoveRange(context.DriverEarnings);
            context.Orders.RemoveRange(context.Orders);
            
            context.CartItems.RemoveRange(context.CartItems);
            context.Carts.RemoveRange(context.Carts);
            context.SearchHistories.RemoveRange(context.SearchHistories);
            context.Notifications.RemoveRange(context.Notifications);
            
            context.MenuItems.RemoveRange(context.MenuItems);
            context.MenuCategories.RemoveRange(context.MenuCategories);
            context.Promotions.RemoveRange(context.Promotions);
            context.CustomerVouchers.RemoveRange(context.CustomerVouchers);
            context.Vouchers.RemoveRange(context.Vouchers);
            context.Restaurants.RemoveRange(context.Restaurants);
            context.FoodCategories.RemoveRange(context.FoodCategories);
            
            await context.SaveChangesAsync();
            Log("Database cleared.");
            Console.WriteLine("Database cleared.");

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
            if (customerUser == null) throw new Exception("Failed to seed Customer user.");
            
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
                    Code = "COM",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1531/1531338.png", 
                    BackgroundColor = "#FFF5E6", 
                    ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Bún/Phở", 
                    Code = "BUN_PHO",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/3421/3421683.png", 
                    BackgroundColor = "#E6F7FF", 
                    ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Trà Sữa", 
                    Code = "TRA_SUA",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/3081/3081162.png", 
                    BackgroundColor = "#FFF0F6", 
                    ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Gà Rán", // Thức ăn nhanh
                    Code = "FAST_FOOD",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/737/737967.png", 
                    BackgroundColor = "#FFFFE6",
                    ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Cà Phê", 
                    Code = "COFFEE",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2935/2935413.png", 
                    BackgroundColor = "#F4E3D7", 
                    ImageUrl = "https://images.unsplash.com/photo-1497935586351-b67a49e012bf" 
                },
                new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Healthy", 
                    Code = "HEALTHY",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2913/2913456.png", 
                    BackgroundColor = "#E6FFFA", 
                    ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd" 
                },
                 new FoodCategory { 
                    Id = Guid.NewGuid(), 
                    Name = "Đồ Uống", 
                    Code = "DRINKS",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2405/2405597.png", 
                    BackgroundColor = "#F9F0FF", 
                    ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e" 
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
                    existing.Code = catDef.Code; // Upsert code
                    existing.IconUrl = catDef.IconUrl;
                    existing.BackgroundColor = catDef.BackgroundColor;
                    existing.ImageUrl = catDef.ImageUrl;
                }
            }
            await context.SaveChangesAsync();
            Log($"Seeded {categoryDefinitions.Count} Global Categories.");


            // ---------------------------------------------------------
            // 3. Restaurants & Menu Items
            // ---------------------------------------------------------
            var cats = await context.FoodCategories.ToListAsync();
            Restaurant? comTamRest = null;

            var targetRestaurants = new List<dynamic>
            {
                new { 
                    MerchantEmail = "xanh@merchant.com", MerchantName = "Chuỗi Nhà Hàng Xanh",
                    Name = "Nhà Hàng Xanh", 
                    Category = "Healthy", 
                    ImageUrl = "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe",
                    CoverImageUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5",
                    Address = "123 Street",
                    Rating = 4.8, RatingCount = 200, DeliveryTime = 20, DeliveryFee = 15000m, MinPrice = 40000m, Distance = 2.5,
                    Tags = new[] { "Vietnam", "Healthy", "Đồ uống" },
                    MenuItems = new List<dynamic> {
                        new { Name = "Phở Bò Đặc Biệt", Price = 65000m, OriginalPrice = 75000m, IsPopular = true, Category = "Món Chính", Desc = "Nước dùng hầm xương 24h" },
                        new { Name = "Gỏi Cuốn Tôm Thịt", Price = 38000m, OriginalPrice = 45000m, IsPopular = true, Category = "Khai Vị", Desc = "Tôm thịt tươi ngon" },
                        new { Name = "Trà Đào Cam Sả", Price = 35000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Đồ Uống", Desc = "Trà đào tươi mát" }
                    }
                },
                new { 
                    MerchantEmail = "comtam@merchant.com", MerchantName = "Cơm Tấm Bà Tám",
                    Name = "Cơm Tấm Sài Gòn", 
                    Category = "Cơm", 
                    ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733",
                    CoverImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836",
                    Address = "123 Nguyễn Văn Cừ, Q.5",
                    Rating = 4.8, RatingCount = 1200, DeliveryTime = 20, DeliveryFee = 15000m, MinPrice = 35000m, Distance = 2.5,
                    Tags = new[] { "Cơm Tấm", "Sườn Nướng", "Ăn Trưa" },
                    MenuItems = new List<dynamic> {
                        new { Name = "Cơm Sườn", Price = 45000m, OriginalPrice = 50000m, IsPopular = true, Category = "Cơm Tấm", Desc = "Cơm sườn nướng than hồng" },
                        new { Name = "Cơm Bì Chả", Price = 40000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Cơm Tấm", Desc = "Cơm bì chả truyền thống" },
                        new { Name = "Canh Khổ Qua", Price = 15000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Canh/Súp", Desc = "Canh khổ qua dồn thịt" },
                        new { Name = "Trà Đá", Price = 2000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Giải Khát", Desc = "Mát lạnh" }
                    }
                },
                new { 
                    MerchantEmail = "kfc@merchant.com", MerchantName = "KFC Vietnam",
                    Name = "KFC - Gà Rán", 
                    Category = "Gà Rán", 
                    ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec",
                    CoverImageUrl = "https://images.unsplash.com/photo-1513639776629-9be61b9a3164",
                    Address = "Lotte Mart, Q.7",
                    Rating = 4.6, RatingCount = 2000, DeliveryTime = 30, DeliveryFee = 20000m, MinPrice = 40000m, Distance = 5.0,
                    Tags = new[] { "Gà Rán", "KFC", "Fast Food" },
                    MenuItems = new List<dynamic> {
                        new { Name = "Combo Gà Rán A", Price = 89000m, OriginalPrice = 99000m, IsPopular = true, Category = "Combo Nhóm", Desc = "2 Gà + 1 Khoai + 1 Pepsi" },
                        new { Name = "Burger Tôm", Price = 45000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Burger", Desc = "Burger tôm giòn tan" },
                         new { Name = "Gà Giòn Cay", Price = 38000m, OriginalPrice = (decimal?)null, IsPopular = true, Category = "Gà Rán", Desc = "Gà rán công thức cay" },
                        new { Name = "Pepsi Tươi", Price = 15000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Thức Uống", Desc = "Ly vừa" }
                    }
                },
                new { 
                    MerchantEmail = "koi@merchant.com", MerchantName = "Koi The Group",
                    Name = "Koí Thé", 
                    Category = "Trà Sữa", 
                    ImageUrl = "https://plus.unsplash.com/premium_photo-1663928246165-1ab1c85ea324?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                    CoverImageUrl = "https://plus.unsplash.com/premium_photo-1663928246165-1ab1c85ea324?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                    Address = "Vivo City, Q.7",
                    Rating = 4.9, RatingCount = 500, DeliveryTime = 15, DeliveryFee = 10000m, MinPrice = 30000m, Distance = 1.0,
                    Tags = new[] { "Trà Sữa", "Macchiato", "Trân Châu" },
                    MenuItems = new List<dynamic> {
                        new { Name = "Hồng Trà Macchiato", Price = 35000m, OriginalPrice = (decimal?)null, IsPopular = true, Category = "Macchiato", Desc = "Size M - Lớp kem béo ngậy" },
                        new { Name = "Lục Trà Sữa", Price = 40000m, OriginalPrice = (decimal?)null, IsPopular = true, Category = "Trà Sữa", Desc = "Trà sữa hương lài" },
                        new { Name = "Trân Châu Hoàng Kim", Price = 10000m, OriginalPrice = (decimal?)null, IsPopular = false, Category = "Topping", Desc = "Dai ngon" }
                    }
                },
            };

            foreach (var restData in targetRestaurants)
            {
                // 1. Merchant Logic
                string mEmail = restData.MerchantEmail;
                string mName = restData.MerchantName;
                
                var mUser = await context.Users.FirstOrDefaultAsync(u => u.Email == mEmail);
                if (mUser == null)
                {
                    mUser = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = mName,
                        Email = mEmail,
                        PhoneNumber = "09" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                        PasswordHash = "merchant",
                        Role = 3,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    context.Users.Add(mUser);
                }
                
                var mProfile = await context.Merchants.FirstOrDefaultAsync(m => m.UserId == mUser.Id);
                if (mProfile == null)
                {
                    mProfile = new Merchant { Id = Guid.NewGuid(), UserId = mUser.Id, BusinessName = mName, ContactEmail = mEmail, ContactPhone = mUser.PhoneNumber, IsActive = true, IsVerified = true, CreatedAt = now };
                    context.Merchants.Add(mProfile);
                }
                
                // 2. Restaurant
                string restName = restData.Name;
                string restCategory = restData.Category;

                var rest = await context.Restaurants.Include(r => r.MenuCategories).ThenInclude(mc => mc.MenuItems).FirstOrDefaultAsync(r => r.Name == restName);

                if (rest == null)
                {
                    rest = new Restaurant { Id = Guid.NewGuid(), MerchantId = mProfile.Id, Name = restName, CreatedAt = now, IsApproved = true, IsOpen = true, TotalOrders = new Random().Next(50, 500), IsTrending = (double)restData.Rating > 4.7 };
                    context.Restaurants.Add(rest);
                }

                rest.MerchantId = mProfile.Id;
                rest.CategoryId = cats.FirstOrDefault(c => c.Name == restCategory)?.Id;
                rest.ImageUrl = restData.ImageUrl;
                try { rest.CoverImageUrl = restData.CoverImageUrl; } catch {}
                rest.Address = restData.Address;
                rest.Rating = (double)restData.Rating;
                rest.RatingCount = (int)restData.RatingCount;
                rest.DeliveryTime = (int)restData.DeliveryTime;
                rest.DeliveryFee = (decimal)restData.DeliveryFee;
                rest.MinPrice = (decimal)restData.MinPrice;
                rest.Distance = (double)restData.Distance;
                rest.Tags = (string[])restData.Tags;

                await context.SaveChangesAsync();
                if (restName == "Cơm Tấm Sài Gòn") comTamRest = rest;

                // 3. Dynamic Menu Categories from Items
                // Identify unique categories for this restaurant from the item data
                var definedCategories = new HashSet<string>();
                foreach(var item in restData.MenuItems) {
                    definedCategories.Add((string)item.Category);
                }

                int sortOrder = 1;
                foreach (var cn in definedCategories)
                {
                    if (!rest.MenuCategories.Any(c => c.Name == cn))
                    {
                        context.MenuCategories.Add(new MenuCategory
                        {
                            Id = Guid.NewGuid(),
                            RestaurantId = rest.Id,
                            Name = cn,
                            DisplayOrder = sortOrder++
                        });
                    }
                }
                await context.SaveChangesAsync();
                
                rest = await context.Restaurants.Include(r => r.MenuCategories).ThenInclude(mc => mc.MenuItems).FirstOrDefaultAsync(r => r.Id == rest.Id);
                if (rest == null) continue;

                // 4. Menu Items
                foreach (var itemData in restData.MenuItems)
                {
                    string targetCatName = itemData.Category;
                    string itemName = itemData.Name;

                    var targetCat = rest.MenuCategories.FirstOrDefault(c => c.Name == targetCatName);
                    if (targetCat != null)
                    {
                        var item = targetCat.MenuItems.FirstOrDefault(i => i.Name == itemName);
                        if (item == null)
                        {
                            item = new MenuItem { Id = Guid.NewGuid(), MenuCategoryId = targetCat.Id, Name = itemName, IsAvailable = true, DisplayOrder = 0 };
                            context.MenuItems.Add(item);
                        }

                        item.Price = (decimal)itemData.Price;
                        item.Description = (string)itemData.Desc;
                        item.ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38";
                        try { item.OriginalPrice = (decimal?)itemData.OriginalPrice; } catch { item.OriginalPrice = null; }
                        try { item.IsPopular = (bool)itemData.IsPopular; } catch { item.IsPopular = false; }
                    }
                }
                await context.SaveChangesAsync();
            }
            Log("Seeded/Updated all target restaurants with UNIQUE Merchant Accounts and DYNAMIC Categories.");

            // ---------------------------------------------------------
            // 6. Seed Orders (Sample History)
            // ---------------------------------------------------------
            bool hasOrders = await context.Orders.AnyAsync();
            if (!hasOrders && customerUser != null && merchantProfile != null) 
            {
                Console.WriteLine("Seeding Sample Orders...");
                
                // Get a restaurant
                // FIX: Use MenuCategories to access items
                // Get a restaurant
                var orderRest = await context.Restaurants
                                    .Include(r => r.MenuCategories)
                                    .ThenInclude(mc => mc.MenuItems)
                                    .FirstOrDefaultAsync(r => r.Name == "Cơm Tấm Sài Gòn");
                                    
                if (orderRest != null && orderRest.MenuCategories.Any())
                {
                    // FIX: Select from MenuCategories
                    var menuItem = orderRest.MenuCategories.SelectMany(mc => mc.MenuItems).FirstOrDefault();
                    if (menuItem != null)
                    {
                        // Get driver for linking to orders
                        var orderDriver = await context.Drivers.FirstOrDefaultAsync(d => d.UserId == driverUser.Id);
                        
                        // Order 1: Completed
                        var customer = await context.Customers.FirstOrDefaultAsync(c => c.UserId == customerUser.Id);
                        if (customer == null) return; // FIX: Use return instead of continue

                        var order1 = new Order
                        {
                            Id = Guid.NewGuid(),
                            OrderNumber = "ORD-" + DateTime.UtcNow.Ticks,
                            CustomerId = customer.Id,
                            RestaurantId = orderRest.Id,
                            DeliveryAddress = "123 Le Loi, Q1",
                            Subtotal = menuItem.Price,
                            DeliveryFee = 15000,
                            TotalAmount = menuItem.Price + 15000,
                            Status = 5, // Completed
                            PaymentMethod = 1, // Cash
                            DriverId = orderDriver?.Id, // Link driver to order
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


                        // Order 2: Active (Delivering)
                        var order2 = new Order
                        {
                            Id = Guid.NewGuid(),
                            OrderNumber = "ORD-" + DateTime.UtcNow.Ticks + "-2",
                            CustomerId = customer.Id, // FIX: Reuse customer object
                            RestaurantId = orderRest.Id,
                            DeliveryAddress = "456 Nguyen Trai, Q5",
                            DeliveryLatitude = 10.755, 
                            DeliveryLongitude = 106.67,
                            Subtotal = menuItem.Price * 2,
                        DeliveryFee = 15000,
                        TotalAmount = (menuItem.Price * 2) + 15000,
                        Status = 4, // Delivering
                        PaymentMethod = 1, // Cash
                        DriverId = orderDriver?.Id, // Link driver to order
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

                // ---------------------------------------------------------
                // 6.1 Seed Driver Earnings (New)
                // ---------------------------------------------------------
                var driver = await context.Drivers.FirstOrDefaultAsync(d => d.UserId == driverUser.Id);
                if (driver != null)
                {
                    var earnings = new List<DriverEarning>();
                    var random = new Random();
                    decimal totalEarnings = 0;

                    // Generate last 30 days of earnings
                    for (int i = 0; i < 30; i++)
                    {
                        var date = now.AddDays(-i);
                        int dailyOrders = random.Next(3, 8); // 3-8 orders/day

                        for (int j = 0; j < dailyOrders; j++)
                        {
                            decimal amount = 15000 + (random.Next(1, 5) * 5000); // 15k to 35k
                            earnings.Add(new DriverEarning
                            {
                                Id = Guid.NewGuid(),
                                DriverId = driver.Id,
                                Amount = amount,
                                Type = 1, // Order Income
                                EarnedAt = date.AddHours(random.Next(8, 22)), // 8 AM - 10 PM
                                CreatedAt = date,
                                Description = $"Thu nhập từ đơn hàng #DH-{random.Next(1000, 9999)}"
                            });
                            totalEarnings += amount;
                        }
                    }

                    // Add some bonuses
                    earnings.Add(new DriverEarning
                    {
                        Id = Guid.NewGuid(),
                        DriverId = driver.Id,
                        Amount = 500000,
                        Type = 2, // Bonus
                        EarnedAt = now.AddDays(-1),
                        CreatedAt = now.AddDays(-1),
                        Description = "Thưởng hoàn thành mốc tuần"
                    });
                    totalEarnings += 500000;

                    await context.DriverEarnings.AddRangeAsync(earnings);
                    
                    driver.WalletBalance = totalEarnings;
                    driver.TotalDeliveries = earnings.Count(e => e.Type == 1);
                    
                    await context.SaveChangesAsync();
                    Log($"Seeded {earnings.Count} driver earning records. Balance: {totalEarnings}");
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
            
            // ---------------------------------------------------------
            // 8. Seed Notifications (Rich Data)
            // ---------------------------------------------------------
            // Clear existing notifications for fresh seed
            var existingNotifs = await context.Notifications.ToListAsync();
            context.Notifications.RemoveRange(existingNotifs);
            await context.SaveChangesAsync();

            var notificationsList = new List<Notification>();
            
            // Customer Notifications
            if (customerUser != null)
            {
                notificationsList.AddRange(new[]
                {
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "🎉 Chào mừng bạn mới!",
                        Message = "Nhập mã WELCOME50 để được giảm 50% cho đơn hàng đầu tiên. Giảm tối đa 50.000đ cho đơn từ 100.000đ.",
                        Type = 2, // Promo
                        ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836",
                        ActionUrl = "/vouchers",
                        IsRead = false,
                        CreatedAt = now.AddDays(-3)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "🍔 Flash Sale - Giảm 30%",
                        Message = "Chỉ hôm nay! Giảm 30% cho tất cả đơn hàng từ KFC. Nhanh tay đặt ngay!",
                        Type = 2, // Promo
                        ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec",
                        ActionUrl = "/restaurants/kfc",
                        IsRead = true,
                        ReadAt = now.AddDays(-2).AddHours(3),
                        CreatedAt = now.AddDays(-2)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "✅ Đơn hàng đã giao thành công",
                        Message = "Đơn hàng #ORD-8392 từ Cơm Tấm Sài Gòn đã được giao thành công. Đánh giá ngay để nhận 10 điểm thưởng!",
                        Type = 1, // Order
                        ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733",
                        ActionUrl = "/orders/history",
                        IsRead = true,
                        ReadAt = now.AddDays(-1).AddHours(2),
                        CreatedAt = now.AddDays(-1)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "🚴 Tài xế đang đến",
                        Message = "Tài xế Nguyễn Văn Tài Xe đang trên đường giao đơn hàng. Dự kiến còn 8 phút nữa sẽ đến.",
                        Type = 1, // Order
                        ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64",
                        ActionUrl = "/orders/tracking",
                        IsRead = false,
                        CreatedAt = now.AddMinutes(-10)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "🔔 Nhà hàng đang chuẩn bị món",
                        Message = "Nhà hàng Cơm Tấm Sài Gòn đã xác nhận và đang chuẩn bị đơn hàng của bạn.",
                        Type = 1, // Order
                        ActionUrl = "/orders/tracking",
                        IsRead = false,
                        CreatedAt = now.AddMinutes(-25)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "💳 Nạp tiền thành công",
                        Message = "Bạn đã nạp thành công 500.000đ vào ví. Số dư hiện tại: 750.000đ.",
                        Type = 3, // System
                        ActionUrl = "/wallet",
                        IsRead = true,
                        ReadAt = now.AddDays(-5).AddHours(1),
                        CreatedAt = now.AddDays(-5)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerUser.Id,
                        Title = "⭐ Đánh giá của bạn đã được ghi nhận",
                        Message = "Cảm ơn bạn đã đánh giá 5 sao cho Koí Thé. Bạn đã nhận được 10 điểm thưởng!",
                        Type = 3, // System
                        ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4",
                        ActionUrl = "/loyalty",
                        IsRead = false,
                        CreatedAt = now.AddHours(-2)
                    }
                });
            }

            // Driver Notifications
            if (driverUser != null)
            {
                notificationsList.AddRange(new[]
                {
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = driverUser.Id,
                        Title = "📦 Đơn hàng mới gần bạn!",
                        Message = "Có đơn hàng mới từ Nhà Hàng Xanh cách bạn 1.2km. Thu nhập dự kiến: 25.000đ.",
                        Type = 1, // Order
                        ImageUrl = "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe",
                        ActionUrl = "/driver/orders/available",
                        IsRead = false,
                        CreatedAt = now.AddMinutes(-3)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = driverUser.Id,
                        Title = "💰 Thưởng hoàn thành mốc tuần",
                        Message = "Chúc mừng! Bạn đã hoàn thành 50 đơn trong tuần và nhận thưởng 500.000đ vào ví.",
                        Type = 2, // Bonus/Promo
                        ImageUrl = "https://images.unsplash.com/photo-1579621970563-ebec7560ff3e",
                        ActionUrl = "/driver/wallet",
                        IsRead = true,
                        ReadAt = now.AddDays(-1).AddHours(5),
                        CreatedAt = now.AddDays(-1)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = driverUser.Id,
                        Title = "⚡ Giờ cao điểm - Thu nhập x1.5",
                        Message = "Từ 11:00 - 13:00 hôm nay, tất cả đơn hàng được nhân 1.5 lần thu nhập. Bật online ngay!",
                        Type = 2, // Promo
                        ActionUrl = "/driver/home",
                        IsRead = false,
                        CreatedAt = now.AddHours(-1)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = driverUser.Id,
                        Title = "✅ Đơn hoàn thành - Thu nhập 35.000đ",
                        Message = "Đơn hàng #ORD-7823 đã giao thành công. 35.000đ đã được cộng vào ví của bạn.",
                        Type = 1, // Order
                        ActionUrl = "/driver/wallet",
                        IsRead = true,
                        ReadAt = now.AddHours(-3),
                        CreatedAt = now.AddHours(-3).AddMinutes(-5)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = driverUser.Id,
                        Title = "📋 Cập nhật chính sách mới",
                        Message = "Từ ngày 01/02, phí dịch vụ sẽ được điều chỉnh. Xem chi tiết để biết thêm.",
                        Type = 3, // System
                        ActionUrl = "/driver/policy",
                        IsRead = false,
                        CreatedAt = now.AddDays(-2)
                    },
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = driverUser.Id,
                        Title = "⭐ Khách hàng đánh giá 5 sao",
                        Message = "Khách hàng Nguyễn Văn Khách đã đánh giá bạn 5 sao: \"Giao hàng nhanh, thái độ tốt!\"",
                        Type = 3, // System
                        ActionUrl = "/driver/ratings",
                        IsRead = false,
                        CreatedAt = now.AddMinutes(-45)
                    }
                });
            }

            await context.Notifications.AddRangeAsync(notificationsList);
            await context.SaveChangesAsync();
            Log($"Seeded {notificationsList.Count} rich Notifications for customer and driver.");

            // ---------------------------------------------------------
            // 9. Seed Chat Messages (Realistic conversations)
            // ---------------------------------------------------------
            var customerForChat = await context.Customers.FirstOrDefaultAsync(c => c.UserId == customerUser.Id);
            if (customerForChat == null) return;
            
            // Get all orders for this customer to add chat messages
            var allOrders = await context.Orders
                .Where(o => o.CustomerId == customerForChat.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            foreach (var order in allOrders)
            {
                if (await context.ChatMessages.AnyAsync(m => m.OrderId == order.Id))
                    continue;

                var chatMessages = new List<ChatMessage>();
                var baseTime = order.CreatedAt;

                if (order.Status == 4) // Đang giao - cuộc hội thoại đang diễn ra
                {
                    chatMessages.AddRange(new[]
                    {
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Chào bạn, mình là tài xế Tài Xe. Mình đã nhận đơn và đang trên đường đến quán nhé! 🏍️",
                            CreatedAt = baseTime.AddMinutes(2),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(3)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Dạ vâng, cảm ơn anh. Anh đến quán rồi nhắn mình nhé!",
                            CreatedAt = baseTime.AddMinutes(3),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(3)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Mình đến quán rồi nha, đang chờ lấy đồ ăn. Quán đông lắm 😅",
                            CreatedAt = baseTime.AddMinutes(8),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(9)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Ok anh, từ từ không sao ạ. Anh nhớ lấy thêm đũa muỗng giúp mình nhé!",
                            CreatedAt = baseTime.AddMinutes(9),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(9)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Được rồi nha! Mình đã lấy đồ xong, đang trên đường giao đến bạn. Khoảng 10-12 phút nữa mình tới 🚀",
                            CreatedAt = baseTime.AddMinutes(14),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(14)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Dạ mình ở tầng 3, phòng 302 nha anh. Anh đến bảo vệ mở cửa giùm",
                            CreatedAt = baseTime.AddMinutes(15),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(15)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Ok bạn! Mình gần tới rồi, còn khoảng 5 phút nữa thôi 📍",
                            CreatedAt = baseTime.AddMinutes(18),
                            IsRead = false
                        }
                    });
                }
                else if (order.Status == 5) // Đã hoàn thành - cuộc hội thoại đầy đủ
                {
                    chatMessages.AddRange(new[]
                    {
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Xin chào! Mình là tài xế vừa nhận đơn của bạn. Mình đang đến quán lấy đồ nhé! 😊",
                            CreatedAt = baseTime.AddMinutes(3),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(4)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Dạ cảm ơn anh! Mình ngồi chờ nha.",
                            CreatedAt = baseTime.AddMinutes(4),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(4)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Mình tới quán rồi nha, quán đang làm đồ. Chắc 5-7 phút là xong.",
                            CreatedAt = baseTime.AddMinutes(10),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(11)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Okela anh, địa chỉ mình là 123 Lê Lợi, Q1 nhé. Có cổng màu xanh.",
                            CreatedAt = baseTime.AddMinutes(11),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(11)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Lấy đồ xong rồi, mình đang ship qua cho bạn đây! 🏃‍♂️",
                            CreatedAt = baseTime.AddMinutes(18),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(18)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Mình tới nơi rồi bạn ơi! Bạn ra cổng nhận đồ nha 📦",
                            CreatedAt = baseTime.AddMinutes(30),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(30)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Mình ra ngay! 1 phút",
                            CreatedAt = baseTime.AddMinutes(31),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(31)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = customerUser.Id,
                            IsFromCustomer = true,
                            Content = "Nhận được rồi anh, cảm ơn anh nhiều nha! Đánh giá 5 sao cho anh ⭐⭐⭐⭐⭐",
                            CreatedAt = baseTime.AddMinutes(33),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(33)
                        },
                        new ChatMessage
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            SenderId = driverUser.Id,
                            IsFromCustomer = false,
                            Content = "Cảm ơn bạn! Chúc bạn ngon miệng nha 😄🍽️",
                            CreatedAt = baseTime.AddMinutes(34),
                            IsRead = true,
                            ReadAt = baseTime.AddMinutes(35)
                        }
                    });
                }

                if (chatMessages.Any())
                {
                    await context.ChatMessages.AddRangeAsync(chatMessages);
                }
            }
            
            await context.SaveChangesAsync();
            Log($"Seeded realistic Chat Messages for {allOrders.Count} orders.");

            Log("Seed completed successfully!");
            Console.WriteLine("Seed completed successfully!");
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : "";
            Log($"Error seeding data: {ex.Message}{detail}\n{ex.StackTrace}");
            Console.WriteLine($"Error seeding data: {ex.Message}");
            throw; 
        }
    }
}
