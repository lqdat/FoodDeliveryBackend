using System;

namespace FoodDeliveryBackend.Core.Entities;

public class CustomerVoucher
{
    public Guid Id { get; set; }
    
    public Guid CustomerId { get; set; }
    
    public Guid VoucherId { get; set; }
    
    public bool IsUsed { get; set; }
    
    public DateTime SavedAt { get; set; }
    
    public DateTime? UsedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    
    public virtual Voucher Voucher { get; set; } = null!;
}
