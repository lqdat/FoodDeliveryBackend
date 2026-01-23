using FoodDeliveryBackend.API.DTOs;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDeliveryBackend.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/wallet")]
    public class CustomerWalletController : ControllerBase
    {
        private readonly FoodDeliveryDbContext _context;

        public CustomerWalletController(FoodDeliveryDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? Guid.Parse(userId) : Guid.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerWalletDto>> GetWalletInfo()
        {
            var userId = GetUserId();
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null) return NotFound("Customer profile not found.");

            // 1. Mock Linked Wallets (In a real app, this would be stored in DB)
            // Simulating MoMo is linked, others are not
            var linkedWallets = new List<LinkedWalletDto>
            {
                new LinkedWalletDto
                {
                    Id = "momo",
                    Name = "MoMo",
                    LogoUrl = "https://cdn-icons-png.flaticon.com/512/888/888870.png", // Placeholder
                    IsLinked = true 
                },
                new LinkedWalletDto
                {
                    Id = "zalopay",
                    Name = "ZaloPay",
                    LogoUrl = "https://cdn-icons-png.flaticon.com/512/888/888871.png", // Placeholder
                    IsLinked = false
                },
                new LinkedWalletDto
                {
                    Id = "shopeepay",
                    Name = "ShopeePay",
                    LogoUrl = "https://cdn-icons-png.flaticon.com/512/888/888872.png", // Placeholder
                    IsLinked = false
                }
            };

            // 2. Mock Bank Cards
            var bankCards = new List<BankCardDto>
            {
                new BankCardDto
                {
                    Id = "card_1",
                    Brand = "VISA",
                    CardNumberLast4 = "1234",
                    CardHolderName = "NGUYEN VAN A"
                }
            };

            // 3. Transactions (From Orders)
            // Filter orders that are Completed (Status 5) or Cancelled (Status 6) which involve payment
            // For MVP, just showing recent orders as transactions
            var transactions = customer.Orders
                .Where(o => o.Status == 5 || o.Status == 6) // Delivered or Cancelled
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new WalletTransactionDto
                {
                    Id = o.Id,
                    Description = $"Đơn hàng {o.OrderNumber} ({GetPaymentMethodName(o.PaymentMethod)})",
                    Amount = -o.TotalAmount, // Expense
                    Date = o.CreatedAt,
                    Status = o.Status == 5 ? "Thành công" : "Đã hoàn tiền",
                    IsPositive = false
                })
                .ToList();

            return Ok(new CustomerWalletDto
            {
                LinkedWallets = linkedWallets,
                BankCards = bankCards,
                Transactions = transactions
            });
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<List<WalletTransactionDto>>> GetTransactions([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var userId = GetUserId();
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return NotFound("Customer profile not found.");

            // Get transactions from Orders
            var transactions = await _context.Orders
                .Where(o => o.CustomerId == customer.Id && (o.Status == 5 || o.Status == 6))
                .OrderByDescending(o => o.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .Select(o => new WalletTransactionDto
                {
                    Id = o.Id,
                    Description = $"Đơn hàng {o.OrderNumber} ({ (o.PaymentMethod == 1 ? "Tiền mặt" : o.PaymentMethod == 2 ? "MoMo" : o.PaymentMethod == 3 ? "ZaloPay" : "Credit Card") })",
                    Amount = -o.TotalAmount,
                    Date = o.CreatedAt,
                    Status = o.Status == 5 ? "Thành công" : "Đã hoàn tiền",
                    IsPositive = false
                })
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpPost("link")]
        public ActionResult LinkWallet([FromBody] LinkWalletRequest request)
        {
            // Mock success
            return Ok(new { message = $"Đã liên kết thành công ví {request.WalletId}" });
        }

        [HttpPost("unlink/{walletId}")]
        public ActionResult UnlinkWallet(string walletId)
        {
            // Mock success
            return Ok(new { message = $"Đã hủy liên kết ví {walletId}" });
        }
        
        [HttpPost("cards")]
        public ActionResult AddCard([FromBody] AddCardRequest request)
        {
             // Mock success
            return Ok(new { message = "Thêm thẻ ngân hàng thành công" });
        }

        [HttpDelete("cards/{cardId}")]
        public ActionResult RemoveCard(string cardId)
        {
             // Mock success
            return Ok(new { message = "Đã xóa thẻ ngân hàng" });
        }

        private string GetPaymentMethodName(int method)
        {
            return method switch
            {
                1 => "Tiền mặt",
                2 => "MoMo",
                3 => "ZaloPay",
                4 => "Credit Card",
                _ => "Khác"
            };
        }
    }
}
