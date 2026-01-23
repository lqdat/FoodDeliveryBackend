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
public class LocationsController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public LocationsController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    private async Task<Customer?> GetCurrentCustomerAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses(
        [FromQuery] string? filter, // "recent" | "saved"
        [FromQuery] string? sortBy) // "recent" | "created" (optional, mainly for "saved" list sorting)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        var query = _context.Addresses
            .Where(a => a.CustomerId == customer.Id && !a.IsDeleted);

        // Apply Filter
        if (filter?.ToLower() == "recent")
        {
            // Only show addresses that have been used
            query = query.Where(a => a.LastUsedAt.HasValue);
            // Default sort for recent is by LastUsedAt
            query = query.OrderByDescending(a => a.LastUsedAt);
        }
        else // "saved" or default
        {
            // Apply Sorting for Saved list
            switch (sortBy?.ToLower())
            {
                case "recent":
                     query = query.OrderByDescending(a => a.LastUsedAt ?? DateTime.MinValue);
                     break;
                case "created":
                    query = query.OrderByDescending(a => a.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(a => a.IsDefault)
                                 .ThenByDescending(a => a.CreatedAt);
                    break;
            }
        }

        var addresses = await query
            .Select(a => new AddressDto
            {
                Id = a.Id,
                Label = a.Label,
                FullAddress = a.FullAddress,
                Note = a.Note,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                IsDefault = a.IsDefault,
                CreatedAt = a.CreatedAt,
                LastUsedAt = a.LastUsedAt
            })
            .ToListAsync();

        return Ok(addresses);
    }

    // Mark an address as "used" (updates LastUsedAt)
    [HttpPost("use/{id}")]
    public async Task<IActionResult> UseAddress(Guid id)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customer.Id);
        if (address == null) return NotFound();

        address.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Address usage updated" });
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        // If setting as default, unset others
        if (request.IsDefault)
        {
            var defaults = await _context.Addresses
                .Where(a => a.CustomerId == customer.Id && a.IsDefault)
                .ToListAsync();
            foreach (var addr in defaults) addr.IsDefault = false;
        }

        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Label = request.Label,
            FullAddress = request.FullAddress,
            Note = request.Note,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();

        return Ok(new AddressDto
        {
            Id = address.Id,
            Label = address.Label,
            FullAddress = address.FullAddress,
            Note = address.Note,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            IsDefault = address.IsDefault
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customer.Id);
        if (address == null) return NotFound();

        address.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Address deleted" });
    }
    
    [HttpPost("set-default/{id}")]
    public async Task<IActionResult> SetDefaultAddress(Guid id)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customer.Id);
        if (address == null) return NotFound();

        // Unset others
        var defaults = await _context.Addresses
            .Where(a => a.CustomerId == customer.Id && a.IsDefault)
            .ToListAsync();
        foreach (var addr in defaults) addr.IsDefault = false;

        address.IsDefault = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Set as default address" });
    }
}
