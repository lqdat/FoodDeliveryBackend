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
public class CartsController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public CartsController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    // GET: api/carts - Get all carts for current user (grouped by restaurant)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartResponse>>> GetMyCarts()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // Find Customer Profile
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));
        if (customer == null) return BadRequest("Customer profile not found for this user.");

        var carts = await _context.Carts
            .Include(c => c.Restaurant)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
            .Where(c => c.CustomerId == customer.Id && !c.IsDeleted)
            .ToListAsync();

        var response = carts.Select(c => new CartResponse
        {
            Id = c.Id,
            RestaurantId = c.RestaurantId,
            RestaurantName = c.Restaurant.Name,
            RestaurantImage = c.Restaurant.ImageUrl,
            Items = c.CartItems.Where(ci => !ci.IsDeleted).Select(ci => new CartItemResponse
            {
                Id = ci.Id,
                MenuItemId = ci.MenuItemId,
                Name = ci.MenuItem.Name,
                ImageUrl = ci.MenuItem.ImageUrl,
                Price = ci.UnitPrice,
                Quantity = ci.Quantity,
                Notes = ci.Notes
            }).ToList(),
            TotalAmount = c.CartItems.Where(ci => !ci.IsDeleted).Sum(ci => ci.Quantity * ci.UnitPrice),
            TotalItems = c.CartItems.Where(ci => !ci.IsDeleted).Sum(ci => ci.Quantity)
        }).ToList();

        return Ok(response);
    }

    // POST: api/carts - Add Item to Cart (Auto-create cart if needed)
    [HttpPost]
    public async Task<ActionResult<CartResponse>> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));
        if (customer == null) return BadRequest("Customer profile not found. Please re-login.");

        // 1. Check if Cart exists for this Restaurant
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id && c.RestaurantId == request.RestaurantId && !c.IsDeleted);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                RestaurantId = request.RestaurantId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Carts.AddAsync(cart);
        }

        // 2. Check if Item exists in Cart
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == request.MenuItemId && !ci.IsDeleted);
        
        // Get MenuItem Price
        var menuItem = await _context.MenuItems.FindAsync(request.MenuItemId);
        if (menuItem == null) return NotFound("Menu Item not found");

        if (cartItem != null)
        {
            cartItem.Quantity += request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem.Notes = request.Notes ?? cartItem.Notes; // Update note if provided
        }
        else
        {
            cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                MenuItemId = request.MenuItemId,
                Quantity = request.Quantity,
                UnitPrice = menuItem.Price,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };
            await _context.CartItems.AddAsync(cartItem);
        }

        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Item added to cart", cartId = cart.Id });
    }

    // DELETE: api/carts/{id} - Clear entire cart
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCart(Guid id)
    {
        var cart = await _context.Carts.FindAsync(id);
        if (cart == null) return NotFound();

        // Soft delete
        cart.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cart cleared" });
    }
}
