using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.API.DTOs
{
    public class CustomerWalletDto
    {
        // Message: "Số dư được quản lý trực tiếp tại ứng dụng của đối tác"
        public List<LinkedWalletDto> LinkedWallets { get; set; } = new();
        public List<BankCardDto> BankCards { get; set; } = new();
        public List<WalletTransactionDto> Transactions { get; set; } = new();
    }

    public class LinkedWalletDto
    {
        public string Id { get; set; } = null!; // e.g., "momo", "zalopay"
        public string Name { get; set; } = null!; // e.g., "MoMo"
        public string LogoUrl { get; set; } = null!;
        public bool IsLinked { get; set; }
    }

    public class BankCardDto
    {
        public string Id { get; set; } = null!;
        public string Brand { get; set; } = null!; // "VISA", "MasterCard"
        public string CardNumberLast4 { get; set; } = null!;
        public string CardHolderName { get; set; } = null!;
    }

    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = null!; // "Đơn hàng #123 (MoMo)"
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = null!; // "Thành công"
        public bool IsPositive { get; set; } // true if + (add funds), false if - (payment)
    }

    public class LinkWalletRequest
    {
        public string WalletId { get; set; } = null!; // "momo"
    }
    
    public class AddCardRequest
    {
        public string CardNumber { get; set; } = null!;
        public string HolderName { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public string Cvv { get; set; } = null!;
    }
}
