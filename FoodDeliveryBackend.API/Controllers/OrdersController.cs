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
    public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders([FromQuery] Guid userId)
    {
         if (userId == Guid.Empty) return BadRequest("UserId is required");

         // Find Customer profile from UserId
         var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
         if (customer == null) return NotFound("Customer profile not found for this user.");

         return await _context.Orders
             .Include(o => o.Restaurant)
             .Include(o => o.OrderItems)
             .Where(o => o.CustomerId == customer.Id)
             .OrderByDescending(o => o.CreatedAt)
             .ToListAsync();
    }
}
