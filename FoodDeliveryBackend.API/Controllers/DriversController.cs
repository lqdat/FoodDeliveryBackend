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
public class DriversController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public DriversController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    private async Task<Driver?> GetCurrentDriverAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        return await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == Guid.Parse(userId));
    }

    // GET: api/drivers/profile
    [HttpGet("profile")]
    public async Task<ActionResult<DriverProfileDto>> GetProfile()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var yearsActive = (int)((DateTime.UtcNow - driver.CreatedAt).TotalDays / 365);
        if (yearsActive < 1) yearsActive = 1; // Minimum 1 year for display or handle as 0

        // Completion rate estimation
        var totalOrders = driver.TotalDeliveries;
        var cancelledOrders = await _context.Orders.CountAsync(o => o.DriverId == driver.Id && o.Status == 6);
        var completionRate = totalOrders + cancelledOrders > 0 
            ? (int)((double)totalOrders / (totalOrders + cancelledOrders) * 100) 
            : 100;

        return new DriverProfileDto
        {
            Id = driver.Id,
            FullName = driver.User.FullName,
            PhoneNumber = driver.User.PhoneNumber,
            AvatarUrl = driver.User.AvatarUrl,
            VehicleType = driver.VehicleType,
            VehicleBrand = driver.VehicleBrand,
            VehiclePlate = driver.VehiclePlate,
            IsOnline = driver.IsOnline,
            IsVerified = driver.IsVerified,
            WalletBalance = driver.WalletBalance,
            Rating = driver.Rating,
            CompletionRate = completionRate,
            YearsActive = yearsActive,
            TotalDeliveries = driver.TotalDeliveries,
            IdentityNumber = driver.IdentityNumber,
            DriverLicenseNumber = driver.DriverLicenseNumber
        };
    }

    // POST: api/drivers/toggle-online
    [HttpPost("toggle-online")]
    public async Task<IActionResult> ToggleOnline()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        driver.IsOnline = !driver.IsOnline;
        driver.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { isOnline = driver.IsOnline });
    }

    // POST: api/drivers/update-location
    [HttpPost("update-location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        driver.CurrentLatitude = request.Latitude;
        driver.CurrentLongitude = request.Longitude;
        driver.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Location updated" });
    }

    // GET: api/drivers/available-orders
    [HttpGet("available-orders")]
    public async Task<ActionResult<IEnumerable<AvailableOrderDto>>> GetAvailableOrders()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        // Status 1 = Confirmed by Restaurant, waiting for driver
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.OrderItems)
            .Where(o => o.Status == 1 && o.DriverId == null && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Take(20)
            .ToListAsync();

        return orders.Select(o => new AvailableOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            RestaurantName = o.Restaurant.Name,
            RestaurantAddress = o.Restaurant.Address,
            DeliveryAddress = o.DeliveryAddress,
            TotalAmount = o.TotalAmount,
            DeliveryFee = o.DeliveryFee,
            Distance = o.Restaurant.Distance,
            ItemCount = o.OrderItems.Count,
            CreatedAt = o.CreatedAt
        }).ToList();
    }

    // POST: api/drivers/accept-order/{orderId}
    [HttpPost("accept-order/{orderId}")]
    public async Task<IActionResult> AcceptOrder(Guid orderId)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return NotFound("Order not found.");
        if (order.DriverId != null) return BadRequest("Order already accepted by another driver.");

        order.DriverId = driver.Id;
        order.Status = 3; // PickingUp
        order.UpdatedAt = DateTime.UtcNow;

        // Add tracking
        await _context.OrderTrackings.AddAsync(new OrderTracking
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = 3,
            Description = "Driver accepted order",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Order accepted" });
    }

    // POST: api/drivers/reject-order/{orderId}
    [HttpPost("reject-order/{orderId}")]
    public async Task<IActionResult> RejectOrder(Guid orderId)
    {
        // For now, just acknowledge - order stays available for other drivers
        return Ok(new { message = "Order rejected, will be offered to other drivers" });
    }

    // GET: api/drivers/active-delivery
    [HttpGet("active-delivery")]
    public async Task<ActionResult<ActiveDeliveryDto>> GetActiveDelivery()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        // Status 3 = PickingUp, 4 = Delivering
        var order = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.DriverId == driver.Id && (o.Status == 3 || o.Status == 4));

        if (order == null) return NotFound("No active delivery.");

        string[] statusTexts = { "Pending", "Confirmed", "Preparing", "PickingUp", "Delivering", "Delivered", "Cancelled" };

        return new ActiveDeliveryDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            StatusText = statusTexts.ElementAtOrDefault(order.Status) ?? "Unknown",
            RestaurantName = order.Restaurant.Name,
            RestaurantAddress = order.Restaurant.Address,
            RestaurantLat = order.Restaurant.Latitude,
            RestaurantLng = order.Restaurant.Longitude,
            CustomerName = order.Customer.User.FullName,
            CustomerPhone = order.Customer.User.PhoneNumber,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLat = order.DeliveryLatitude,
            DeliveryLng = order.DeliveryLongitude,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Name = oi.MenuItem?.Name ?? oi.ItemName ?? "Unknown",
                Quantity = oi.Quantity
            }).ToList()
        };
    }

    // POST: api/drivers/update-order-status/{orderId}
    [HttpPost("update-order-status/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return NotFound("Order not found.");
        if (order.DriverId != driver.Id) return Unauthorized("This order is not assigned to you.");

        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (request.NewStatus == 4) order.PickedUpAt = DateTime.UtcNow;
        if (request.NewStatus == 5) 
        {
            order.DeliveredAt = DateTime.UtcNow;
            driver.TotalDeliveries += 1;
            
            // Add earning
            await _context.DriverEarnings.AddAsync(new DriverEarning
            {
                Id = Guid.NewGuid(),
                DriverId = driver.Id,
                OrderId = orderId,
                Amount = order.DeliveryFee * 0.8m, // 80% of delivery fee
                CreatedAt = DateTime.UtcNow
            });
            driver.WalletBalance += order.DeliveryFee * 0.8m;
        }

        await _context.OrderTrackings.AddAsync(new OrderTracking
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = request.NewStatus,
            Description = $"Status updated to {request.NewStatus}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Order status updated" });
    }

    // GET: api/drivers/income/summary?period=month
    [HttpGet("income/summary")]
    public async Task<ActionResult<DriverIncomeSummaryDto>> GetIncomeSummary([FromQuery] string period = "month")
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var now = DateTime.UtcNow;
        DateTime startDate;
        string label;

        if (period.ToLower() == "week")
        {
            startDate = now.AddDays(-(int)now.DayOfWeek);
            label = "Tuần này";
        }
        else if (period.ToLower() == "year")
        {
            startDate = new DateTime(now.Year, 1, 1);
            label = $"Năm {now.Year}";
        }
        else // month
        {
            startDate = new DateTime(now.Year, now.Month, 1);
            label = $"Tháng {now.Month}, {now.Year}";
        }

        var earnings = await _context.DriverEarnings
            .Where(e => e.DriverId == driver.Id && e.EarnedAt >= startDate)
            .ToListAsync();

        var totalIncome = earnings.Sum(e => e.Amount);
        var orderCount = await _context.Orders.CountAsync(o => o.DriverId == driver.Id && o.Status == 5 && o.DeliveredAt >= startDate);
        var totalDistance = await _context.Orders.Where(o => o.DriverId == driver.Id && o.Status == 5 && o.DeliveredAt >= startDate).SumAsync(o => o.Distance);

        // Mock chart data
        var chartData = new List<IncomeChartPointDto>();
        for (int i = 1; i <= 4; i++)
        {
            decimal mockValue = totalIncome * (0.1m + (i * 0.05m));
            chartData.Add(new IncomeChartPointDto { Label = $"Tuần {i}", Value = mockValue });
        }

        return new DriverIncomeSummaryDto
        {
            PeriodLabel = label,
            TotalIncome = totalIncome,
            GrowthPercentage = 12.5, // Mock
            CompletedOrders = orderCount,
            TotalDistanceKm = totalDistance,
            MonthlyBonus = 500000, // Mock
            ChartData = chartData
        };
    }

    // GET: api/drivers/income/history
    [HttpGet("income/history")]
    public async Task<ActionResult<IEnumerable<DriverIncomeHistoryGroupDto>>> GetIncomeHistory()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var earnings = await _context.DriverEarnings
            .Include(e => e.Order)
                .ThenInclude(o => o!.Restaurant)
            .Where(e => e.DriverId == driver.Id)
            .OrderByDescending(e => e.EarnedAt)
            .Take(50)
            .ToListAsync();

        var groups = earnings.GroupBy(e => e.EarnedAt.Date)
            .Select(g => new DriverIncomeHistoryGroupDto
            {
                DateLabel = g.Key == DateTime.UtcNow.Date ? "Hôm nay" : g.Key == DateTime.UtcNow.Date.AddDays(-1) ? "Hôm qua" : g.Key.ToString("dd/MM/yyyy"),
                DateValue = g.Key.ToString("dd/MM"),
                OrderCount = g.Count(e => e.OrderId != null),
                TotalAmount = g.Sum(e => e.Amount),
                Items = g.Select(e => new DriverIncomeItemDto
                {
                    OrderId = e.OrderId ?? Guid.Empty,
                    OrderNumber = e.Order?.OrderNumber ?? "N/A",
                    MerchantName = e.Order?.Restaurant?.Name ?? "Hệ thống",
                    Time = e.EarnedAt.ToLocalTime().ToString("hh:mm tt"),
                    DistanceKm = e.Order?.Distance ?? 0,
                    Amount = e.Amount
                }).ToList()
            }).ToList();

        return groups;
    }

    // GET: api/drivers/documents
    [HttpGet("documents")]
    public async Task<ActionResult<IEnumerable<DriverDocumentDto>>> GetDocuments()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        return new List<DriverDocumentDto>
        {
            new DriverDocumentDto { Type = "CI", Name = "Căn cước công dân", Status = "Approved", ExpiryDate = new DateTime(2030, 10, 20), DocumentUrl = driver.IdCardFrontUrl },
            new DriverDocumentDto { Type = "DL", Name = "Bằng lái xe (B2)", Status = "Approved", ExpiryDate = driver.LicenseExpiryDate, DocumentUrl = driver.DriverLicenseUrl },
            new DriverDocumentDto { Type = "CriminalRecord", Name = "Lý lịch tư pháp", Status = "Pending", DocumentUrl = driver.CriminalRecordUrl },
            new DriverDocumentDto { Type = "Insurance", Name = "Bảo hiểm xe", Status = driver.InsuranceExpiryDate < DateTime.UtcNow ? "Expired" : "Approved", ExpiryDate = driver.InsuranceExpiryDate, DocumentUrl = driver.InsuranceUrl },
            new DriverDocumentDto { Type = "Registration", Name = "Giấy đăng ký xe", Status = "Approved", ExpiryDate = driver.RegistrationExpiryDate, DocumentUrl = driver.VehicleRegistrationUrl }
        };
    }

    // GET: api/drivers/order-history
    [HttpGet("order-history")]
    public async Task<ActionResult<IEnumerable<AvailableOrderDto>>> GetOrderHistory()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.OrderItems)
            .Where(o => o.DriverId == driver.Id && o.Status == 5) // Delivered
            .OrderByDescending(o => o.DeliveredAt)
            .Take(50)
            .ToListAsync();

        return orders.Select(o => new AvailableOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            RestaurantName = o.Restaurant.Name,
            RestaurantAddress = o.Restaurant.Address,
            DeliveryAddress = o.DeliveryAddress,
            TotalAmount = o.TotalAmount,
            DeliveryFee = o.DeliveryFee,
            Distance = o.Restaurant.Distance,
            ItemCount = o.OrderItems.Count,
            CreatedAt = o.CreatedAt
        }).ToList();
    }

    // GET: api/drivers/wallet
    [HttpGet("wallet")]
    public async Task<ActionResult<DriverWalletDto>> GetWallet()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var earnings = await _context.DriverEarnings
            .Where(e => e.DriverId == driver.Id && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var todayEarnings = earnings.Where(e => e.EarnedAt.Date == today).Sum(e => e.Amount);
        var weekEarnings = earnings.Where(e => e.EarnedAt.Date >= startOfWeek).Sum(e => e.Amount);
        var monthEarnings = earnings.Where(e => e.EarnedAt.Date >= startOfMonth).Sum(e => e.Amount);

        var transactions = earnings.Take(50).Select(e => new DriverTransactionDto
        {
            Id = e.Id,
            Type = "earning", // Standard for now
            Amount = e.Amount,
            Description = e.Description ?? $"Earnings from Order #{e.OrderId}",
            CreatedAt = e.CreatedAt
        }).ToList();

        return new DriverWalletDto
        {
            Balance = driver.WalletBalance,
            TodayEarnings = todayEarnings,
            WeekEarnings = weekEarnings,
            MonthEarnings = monthEarnings,
            PendingWithdrawal = 0, // Not implemented yet
            Transactions = transactions
        };
    }
}
