namespace FoodDeliveryBackend.API.DTOs;

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Type { get; set; } // 0: Fixed Amount, 1: Percentage
    public decimal DiscountValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime EndDate { get; set; }
    public string? IconUrl { get; set; }
    public bool IsSaved { get; set; } // Helper for UI
}

public class SaveVoucherRequest
{
    public Guid VoucherId { get; set; }
}
