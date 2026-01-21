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

        return new DriverProfileDto
        {
            Id = driver.Id,
            FullName = driver.User.FullName,
            PhoneNumber = driver.User.PhoneNumber,
            AvatarUrl = driver.User.AvatarUrl,
            VehicleType = driver.VehicleType,
            LicensePlate = driver.LicensePlate,
            IsOnline = driver.IsOnline,
            IsVerified = driver.IsVerified,
            WalletBalance = driver.WalletBalance,
            Rating = driver.Rating,
            TotalDeliveries = driver.TotalDeliveries
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

    // GET: api/drivers/wallet
    [HttpGet("wallet")]
    public async Task<ActionResult<DriverWalletDto>> GetWallet()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return NotFound("Driver profile not found.");

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        var earnings = await _context.DriverEarnings
            .Include(e => e.Order)
            .Where(e => e.DriverId == driver.Id)
            .OrderByDescending(e => e.CreatedAt)
            .Take(20)
            .ToListAsync();

        return new DriverWalletDto
        {
            Balance = driver.WalletBalance,
            TodayEarnings = earnings.Where(e => e.CreatedAt.Date == today).Sum(e => e.Amount),
            WeekEarnings = earnings.Where(e => e.CreatedAt.Date >= weekStart).Sum(e => e.Amount),
            RecentEarnings = earnings.Select(e => new DriverEarningDto
            {
                OrderId = e.OrderId ?? Guid.Empty,
                OrderNumber = e.Order?.OrderNumber ?? "N/A",
                Amount = e.Amount,
                CreatedAt = e.CreatedAt
            }).ToList()
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
}
