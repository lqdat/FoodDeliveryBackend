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
public class VouchersController : ControllerBase
{
    private readonly FoodDeliveryDbContext _context;

    public VouchersController(FoodDeliveryDbContext context)
    {
        _context = context;
    }

    private async Task<Customer?> GetCurrentCustomerAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;
        return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));
    }

    // GET: api/vouchers - List all active vouchers (Global list)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VoucherDto>>> GetAllVouchers()
    {
        var customer = await GetCurrentCustomerAsync();
        
        // Get all active vouchers
        var vouchers = await _context.Vouchers
            .Where(v => !v.IsDeleted && v.IsActive && v.EndDate > DateTime.UtcNow)
            .OrderBy(v => v.EndDate)
            .ToListAsync();

        // Check which ones are saved
        List<Guid> savedIds = new();
        if (customer != null)
        {
            savedIds = await _context.CustomerVouchers
                .Where(cv => cv.CustomerId == customer.Id)
                .Select(cv => cv.VoucherId)
                .ToListAsync();
        }

        var dtos = vouchers.Select(v => new VoucherDto
        {
            Id = v.Id,
            Code = v.Code,
            Name = v.Name,
            Description = v.Description,
            Type = v.Type,
            DiscountValue = v.DiscountValue,
            MaxDiscountAmount = v.MaxDiscountAmount,
            MinOrderAmount = v.MinOrderAmount,
            EndDate = v.EndDate,
            IconUrl = v.IconUrl,
            IsSaved = savedIds.Contains(v.Id)
        }).ToList();

        return Ok(dtos);
    }

    // POST: api/vouchers/save - Save a voucher to My Vouchers
    [HttpPost("save")]
    public async Task<IActionResult> SaveVoucher([FromBody] SaveVoucherRequest request)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        var voucher = await _context.Vouchers.FindAsync(request.VoucherId);
        if (voucher == null || voucher.IsDeleted || !voucher.IsActive) 
            return NotFound("Voucher not found or inactive.");

        if (voucher.EndDate < DateTime.UtcNow)
            return BadRequest("Voucher has expired.");

        // Check if already saved
        var exists = await _context.CustomerVouchers
            .AnyAsync(cv => cv.CustomerId == customer.Id && cv.VoucherId == request.VoucherId);
        
        if (exists) return BadRequest("You have already saved this voucher.");

        var customerVoucher = new CustomerVoucher
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            VoucherId = request.VoucherId,
            SavedAt = DateTime.UtcNow,
            IsUsed = false
        };

        await _context.CustomerVouchers.AddAsync(customerVoucher);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Voucher saved successfully." });
    }

    // GET: api/vouchers/my-vouchers - List saved vouchers
    [HttpGet("my-vouchers")]
    public async Task<ActionResult<IEnumerable<VoucherDto>>> GetMyVouchers()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null) return Unauthorized();

        var myVouchers = await _context.CustomerVouchers
            .Include(cv => cv.Voucher)
            .Where(cv => cv.CustomerId == customer.Id && !cv.IsUsed && cv.Voucher.EndDate > DateTime.UtcNow)
            .Select(cv => cv.Voucher)
            .ToListAsync();

        var dtos = myVouchers.Select(v => new VoucherDto
        {
            Id = v.Id,
            Code = v.Code,
            Name = v.Name,
            Description = v.Description,
            Type = v.Type,
            DiscountValue = v.DiscountValue,
            MaxDiscountAmount = v.MaxDiscountAmount,
            MinOrderAmount = v.MinOrderAmount,
            EndDate = v.EndDate,
            IconUrl = v.IconUrl,
            IsSaved = true
        }).ToList();

        return Ok(dtos);
    }
}
