using FoodDeliveryBackend.Core.Entities;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Added specifically for OrderBy

namespace FoodDeliveryBackend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public OrdersController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        // Generate OrderNumber (Unique Index required)
        // OrderNumber is string per schema. Simple unique generation:
        order.OrderNumber = "ORD-" + DateTime.UtcNow.Ticks;
        
        // Ensure ID
        if (order.Id == Guid.Empty) order.Id = Guid.NewGuid();
        
        // Set creation time
        if (order.CreatedAt == default) order.CreatedAt = DateTime.UtcNow;

        // Map UserId to CustomerId if provided
        // Assuming Order comes with UserId (from Frontend User ID) but Schema needs CustomerId
        if (order.CustomerId == Guid.Empty)
        {
             // Strategy: Select the first available customer for this mock backend
             var firstCustomer = await _context.Customers.FirstOrDefaultAsync();
             if (firstCustomer != null) order.CustomerId = firstCustomer.Id;
        }

        _context.Orders.Add(order);
        
        // Handle Order Items
        foreach(var item in order.OrderItems) {
             if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
             item.OrderId = order.Id;
        }
        
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Restaurant)
            .Include(o => o.Driver)
            .Include(o => o.OrderTrackings) // Load trackings
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        // Sort in memory
        order.OrderTrackings = order.OrderTrackings.OrderBy(t => t.CreatedAt).ToList();

        return order;
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders(
        [FromQuery] Guid userId,
        [FromQuery] int? status, // 0=Pending, 1=Confirmed, 2=Preparing, 3=PickingUp, 4=Delivering, 5=Delivered, 6=Cancelled
        [FromQuery] string? search) // Search by Restaurant Name or Order Number
    {
         if (userId == Guid.Empty) return BadRequest("UserId is required");

         // Find Customer profile from UserId
         var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
         if (customer == null) return NotFound("Customer profile not found for this user.");

         var query = _context.Orders
             .Include(o => o.Restaurant)
             .Include(o => o.OrderItems)
             .Where(o => o.CustomerId == customer.Id);

         // Filter by Status
         if (status.HasValue)
         {
             query = query.Where(o => o.Status == status.Value);
         }

         // Search by Restaurant Name or Order Number
         if (!string.IsNullOrWhiteSpace(search))
         {
             search = search.ToLower().Trim();
             query = query.Where(o => 
                 o.Restaurant.Name.ToLower().Contains(search) ||
                 o.OrderNumber.ToLower().Contains(search));
         }

         return await query
             .OrderByDescending(o => o.CreatedAt)
             .ToListAsync();
    }

    // Cancel Order - Only if Pending/Confirmed (0 or 1) and no Driver assigned
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound("Order not found.");

        // Check if order can be cancelled
        // Status: 0=Pending, 1=Confirmed. DriverId must be null (no driver picked up yet)
        if (order.Status > 1)
        {
            return BadRequest("Cannot cancel order. Order is already being prepared or delivered.");
        }

        if (order.DriverId != null)
        {
            return BadRequest("Cannot cancel order. A driver has already accepted this order.");
        }

        order.Status = 6; // Cancelled
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Add tracking entry
        var tracking = new OrderTracking
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = 6,
            Description = "Order cancelled by customer",
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderTrackings.Add(tracking);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Order cancelled successfully" });
    }

    // Reorder - Create a new order based on an existing order
    [HttpPost("{id}/reorder")]
    public async Task<ActionResult<Order>> Reorder(Guid id)
    {
        var originalOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (originalOrder == null) return NotFound("Original order not found.");

        // Create new order based on original
        var newOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-" + DateTime.UtcNow.Ticks,
            CustomerId = originalOrder.CustomerId,
            RestaurantId = originalOrder.RestaurantId,
            DeliveryAddress = originalOrder.DeliveryAddress,
            DeliveryLatitude = originalOrder.DeliveryLatitude,
            DeliveryLongitude = originalOrder.DeliveryLongitude,
            DeliveryNote = originalOrder.DeliveryNote,
            DeliveryFee = originalOrder.DeliveryFee,
            Status = 0, // Pending
            PaymentMethod = originalOrder.PaymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        // Copy order items
        decimal subtotal = 0;
        foreach (var item in originalOrder.OrderItems)
        {
            var newItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                MenuItemId = item.MenuItemId,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                Notes = item.Notes,
                CreatedAt = DateTime.UtcNow
            };
            subtotal += newItem.TotalPrice;
            _context.OrderItems.Add(newItem);
        }

        newOrder.Subtotal = subtotal;
        newOrder.TotalAmount = subtotal + newOrder.DeliveryFee;

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
    }
}
