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
            Log($"Seeded/Updated {categoryDefinitions.Count} categories with Icons and Colors.");


            // ---------------------------------------------------------
            // 3. Restaurants & Menu Items
            // ---------------------------------------------------------
            // ---------------------------------------------------------
            // 3. Restaurants & Menu Items (Consolidated & Robust)
            // ---------------------------------------------------------
            if (merchantProfile != null)
            {
                var cats = await context.FoodCategories.ToListAsync();
                Restaurant? comTamRest = null;
                var targetRestaurants = new List<dynamic>
                {
                    new { 
                        Name = "Cơm Tấm Sài Gòn", 
                        Category = "Cơm", 
                        ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733",
                        Address = "123 Nguyễn Văn Cừ, Q.5",
                        Rating = 4.8, RatingCount = 1200, DeliveryTime = 20, DeliveryFee = 15000m, MinPrice = 35000m, Distance = 2.5,
                        Tags = new[] { "Cơm Tấm", "Sườn Nướng", "Ăn Trưa" },
                        MenuItems = new List<dynamic> {
                            new { Name = "Cơm Sườn", Price = 45000m, Category = "Món Chính", Desc = "Cơm sườn nướng than hồng" },
                            new { Name = "Cơm Bì Chả", Price = 40000m, Category = "Món Chính", Desc = "Cơm bì chả truyền thống" },
                            new { Name = "Cơm Gà Xối Mỡ", Price = 42000m, Category = "Món Chính", Desc = "Gà chiên giòn rụm" },
                            new { Name = "Cơm Ba Rọi Nướng", Price = 48000m, Category = "Món Chính", Desc = "Ba rọi nướng đậm đà" },
                            new { Name = "Cơm Sườn Non Kho", Price = 50000m, Category = "Món Chính", Desc = "Sườn non kho tộ" },
                            new { Name = "Canh Khổ Qua", Price = 15000m, Category = "Món Phụ", Desc = "Canh khổ qua nhồi thịt" },
                            new { Name = "Trứng Ốp La", Price = 5000m, Category = "Món Phụ", Desc = "Trứng gà ta" },
                            new { Name = "Lạp Xưởng", Price = 10000m, Category = "Món Phụ", Desc = "1 cây lạp xưởng tươi" },
                            new { Name = "Canh Rong Biển", Price = 12000m, Category = "Món Phụ", Desc = "Canh rong biển thịt bằm" },
                            new { Name = "Trà Đá", Price = 2000m, Category = "Đồ Uống", Desc = "Mát lạnh" },
                            new { Name = "Nước Sâm", Price = 10000m, Category = "Đồ Uống", Desc = "Sâm lạnh nhà nấu" }
                        }
                    },
                    new { 
                        Name = "KFC - Gà Rán", 
                        Category = "Ăn Vặt", 
                        ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec",
                        Address = "Lotte Mart, Q.7",
                        Rating = 4.6, RatingCount = 2000, DeliveryTime = 30, DeliveryFee = 20000m, MinPrice = 40000m, Distance = 5.0,
                        Tags = new[] { "Gà Rán", "KFC", "Fast Food" },
                        MenuItems = new List<dynamic> {
                            new { Name = "Combo Gà Rán A", Price = 89000m, Category = "Món Chính", Desc = "2 Gà + 1 Khoai + 1 Pepsi" },
                            new { Name = "Combo Gà Rán B", Price = 159000m, Category = "Món Chính", Desc = "4 Gà + 2 Khoai + 2 Pepsi" },
                            new { Name = "Burger Tôm", Price = 45000m, Category = "Món Chính", Desc = "Burger tôm giòn tan" },
                            new { Name = "Burger Zinger", Price = 59000m, Category = "Món Chính", Desc = "Burger gà cay trứ danh" },
                            new { Name = "Cơm Gà Quay", Price = 55000m, Category = "Món Chính", Desc = "Cơm gà quay sốt tiêu" },
                            new { Name = "Mỳ Ý Gà Viên", Price = 45000m, Category = "Món Chính", Desc = "Mỳ ý sốt gà viên" },
                            new { Name = "Khoai Tây Chiên (Vừa)", Price = 25000m, Category = "Món Phụ", Desc = "Giòn rụm" },
                            new { Name = "Khoai Tây Chiên (Lớn)", Price = 35000m, Category = "Món Phụ", Desc = "Size lớn chia sẻ" },
                            new { Name = "Gà Popcorn", Price = 39000m, Category = "Món Phụ", Desc = "Gà viên vui miệng" },
                            new { Name = "Salad Bắp Cải", Price = 15000m, Category = "Món Phụ", Desc = "Coleslaw tươi mát" },
                            new { Name = "Khoai Tây Nghiền", Price = 19000m, Category = "Món Phụ", Desc = "Khoai tây nghiền sốt nâu" },
                            new { Name = "Pepsi Tươi", Price = 15000m, Category = "Đồ Uống", Desc = "Ly vừa" },
                            new { Name = "7Up", Price = 15000m, Category = "Đồ Uống", Desc = "Ly vừa" }
                        }
                    },
                    new { 
                        Name = "Koí Thé", 
                        Category = "Trà Sữa", 
                        ImageUrl = "https://images.unsplash.com/photo-1558359250-9aa4e09f5fa4",
                        Address = "Vivo City, Q.7",
                        Rating = 4.9, RatingCount = 500, DeliveryTime = 15, DeliveryFee = 10000m, MinPrice = 30000m, Distance = 1.0,
                        Tags = new[] { "Trà Sữa", "Macchiato", "Trân Châu" },
                        MenuItems = new List<dynamic> {
                            new { Name = "Hồng Trà Macchiato", Price = 35000m, Category = "Món Chính", Desc = "Size M - Lớp kem béo ngậy" },
                            new { Name = "Lục Trà Trân Châu", Price = 40000m, Category = "Món Chính", Desc = "Thơm ngon đậm vị" },
                            new { Name = "Sữa Tươi Trân Châu", Price = 55000m, Category = "Món Chính", Desc = "Đường đen Tiger" },
                            new { Name = "Oolong Macchiato", Price = 42000m, Category = "Món Chính", Desc = "Trà Oolong đậm đà" },
                            new { Name = "Trà Xanh Chanh Dây", Price = 45000m, Category = "Món Chính", Desc = "Chua ngọt sảng khoái" },
                            new { Name = "Matcha Latte", Price = 52000m, Category = "Món Chính", Desc = "Matcha Nhật Bản" },
                            new { Name = "Trân Châu Hoàng Kim", Price = 10000m, Category = "Món Phụ", Desc = "Dai ngon" },
                            new { Name = "Thạch Dừa", Price = 8000m, Category = "Món Phụ", Desc = "Giòn giòn" },
                            new { Name = "Lô Hội", Price = 8000m, Category = "Món Phụ", Desc = "Tươi mát" },
                            new { Name = "Konjac Jelly", Price = 12000m, Category = "Món Phụ", Desc = "Thạch dẻo" },
                            new { Name = "Trà Đào", Price = 45000m, Category = "Đồ Uống", Desc = "Có miếng đào tươi" }
                        }
                    },
                    new { 
                        Name = "Pizza Hut", 
                        Category = "Ăn Vặt", 
                        ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38",
                        Address = "345 Nguyễn Thị Thập, Q.7",
                        Rating = 4.5, RatingCount = 850, DeliveryTime = 40, DeliveryFee = 25000m, MinPrice = 100000m, Distance = 3.2,
                        Tags = new[] { "Pizza", "Mỳ Ý", "Fast Food" },
                        MenuItems = new List<dynamic> {
                            new { Name = "Pizza Hải Sản (M)", Price = 159000m, Category = "Món Chính", Desc = "Tôm, mực, thanh cua" },
                            new { Name = "Pizza Hải Sản (L)", Price = 239000m, Category = "Món Chính", Desc = "Size Lớn" },
                            new { Name = "Mỳ Ý Bò Bằm", Price = 89000m, Category = "Món Chính", Desc = "Sốt bò bằm cà chua" },
                            new { Name = "Pizza Pepperoni", Price = 139000m, Category = "Món Chính", Desc = "Xúc xích Ý cay nhẹ" },
                            new { Name = "Pizza Phô Mai Cao Cấp", Price = 149000m, Category = "Món Chính", Desc = "3 loại phô mai" },
                            new { Name = "Pizza Rau Củ", Price = 119000m, Category = "Món Chính", Desc = "Dành cho người ăn chay" },
                            new { Name = "Salad Cá Ngừ", Price = 59000m, Category = "Món Phụ", Desc = "Rau tươi sốt mayo" },
                            new { Name = "Bánh Mì Bơ Tỏi", Price = 39000m, Category = "Món Phụ", Desc = "Thơm lừng" },
                            new { Name = "Khoai Tây Cười", Price = 45000m, Category = "Món Phụ", Desc = "Vui nhộn cho bé" },
                            new { Name = "Mực Chiên Giòn", Price = 79000m, Category = "Món Phụ", Desc = "Mực vòng chiên bột" },
                            new { Name = "Coca Cola", Price = 20000m, Category = "Đồ Uống", Desc = "Chai 390ml" },
                            new { Name = "Sprite", Price = 20000m, Category = "Đồ Uống", Desc = "Chai 390ml" }
                        }
                    },
                    new { 
                        Name = "Highlands Coffee", 
                        Category = "Trà Sữa", 
                        ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3",
                        Address = "Crescent Mall, Q.7",
                        Rating = 4.7, RatingCount = 1500, DeliveryTime = 25, DeliveryFee = 15000m, MinPrice = 29000m, Distance = 1.5,
                        Tags = new[] { "Cà Phê", "Trà", "Bánh" },
                        MenuItems = new List<dynamic> {
                            new { Name = "Phin Sữa Đá", Price = 29000m, Category = "Món Chính", Desc = "Cà phê phin truyền thống" },
                            new { Name = "Phin Đen Đá", Price = 29000m, Category = "Món Chính", Desc = "Đậm đà tỉnh táo" },
                            new { Name = "Trà Sen Vàng", Price = 45000m, Category = "Món Chính", Desc = "Trà sen kem sữa" },
                            new { Name = "Freeze Trà Xanh", Price = 55000m, Category = "Món Chính", Desc = "Đá xay mát lạnh" },
                            new { Name = "Trà Thạch Đào", Price = 45000m, Category = "Món Chính", Desc = "Thanh mát giải nhiệt" },
                            new { Name = "Phindi Hạnh Nhân", Price = 42000m, Category = "Món Chính", Desc = "Hương hạnh nhân thơm béo" },
                            new { Name = "Bánh Mì Thịt Nướng", Price = 19000m, Category = "Món Phụ", Desc = "Bánh mì Việt Nam" },
                            new { Name = "Bánh Mì Xíu Mại", Price = 19000m, Category = "Món Phụ", Desc = "Xíu mại sốt cà" },
                            new { Name = "Mousse Đào", Price = 35000m, Category = "Món Phụ", Desc = "Bánh ngọt tráng miệng" },
                            new { Name = "Bánh Chuối", Price = 25000m, Category = "Món Phụ", Desc = "Bánh chuối nướng" },
                            new { Name = "Phô Mai Cà Phê", Price = 29000m, Category = "Món Phụ", Desc = "Bánh phô mai vị cafe" },
                            new { Name = "Bạc Xỉu", Price = 29000m, Category = "Đồ Uống", Desc = "Nhiều sữa ít cafe" },
                            new { Name = "Sữa Tươi", Price = 25000m, Category = "Đồ Uống", Desc = "Sữa tươi Vinamilk" }
                        }
                    }
                };

                foreach (var restData in targetRestaurants)
                {
                    string restName = restData.Name;
                    string restCategory = restData.Category;

                    // 1. Upsert Restaurant
                    var rest = await context.Restaurants
                                    .Include(r => r.MenuCategories)
                                    .ThenInclude(mc => mc.MenuItems)
                                    .FirstOrDefaultAsync(r => r.Name == restName);

                    if (rest == null)
                    {
                        rest = new Restaurant
                        {
                            Id = Guid.NewGuid(),
                            MerchantId = merchantProfile.Id,
                            Name = restName,
                            CreatedAt = now,
                            IsApproved = true,
                            IsOpen = true
                        };
                        context.Restaurants.Add(rest);
                    }

                    // Update fields
                    rest.MerchantId = merchantProfile.Id; // Ensure ownership
                    rest.CategoryId = cats.FirstOrDefault(c => c.Name == restCategory)?.Id;
                    rest.ImageUrl = restData.ImageUrl;
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

                    // 2. Ensure Menu Categories
                    var catNames = new[] { "Món Chính", "Món Phụ", "Đồ Uống" };
                    foreach (var cn in catNames)
                    {
                        if (!rest.MenuCategories.Any(c => c.Name == cn))
                        {
                            context.MenuCategories.Add(new MenuCategory
                            {
                                Id = Guid.NewGuid(),
                                RestaurantId = rest.Id,
                                Name = cn,
                                DisplayOrder = Array.IndexOf(catNames, cn) + 1
                            });
                        }
                    }
                    await context.SaveChangesAsync();
                    
                    // Reload to get new categories
                    rest = await context.Restaurants
                                    .Include(r => r.MenuCategories)
                                    .ThenInclude(mc => mc.MenuItems)
                                    .FirstOrDefaultAsync(r => r.Id == rest.Id);

                    // 3. Upsert Menu Items
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
                                item = new MenuItem
                                {
                                    Id = Guid.NewGuid(),
                                    MenuCategoryId = targetCat.Id,
                                    Name = itemName
                                };
                                context.MenuItems.Add(item);
                            }

                            item.Price = (decimal)itemData.Price;
                            item.Description = (string)itemData.Desc;
                            item.ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38"; // Placeholder
                        }
                    }
                    await context.SaveChangesAsync();
                }

                Log("Seeded/Updated all 5 target restaurants and their complete menus.");
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
            if (customerUser != null)
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
            var detail = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : "";
            Log($"Error seeding data: {ex.Message}{detail}\n{ex.StackTrace}");
            Console.WriteLine($"Error seeding data: {ex.Message}");
            throw; 
        }
    }
}
