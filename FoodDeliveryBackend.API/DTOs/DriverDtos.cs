namespace FoodDeliveryBackend.API.DTOs;

public class DriverProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string VehicleType { get; set; } = null!;
    public string? VehicleBrand { get; set; }
    public string VehiclePlate { get; set; } = null!;
    public bool IsOnline { get; set; }
    public bool IsVerified { get; set; }
    public decimal WalletBalance { get; set; }
    public double Rating { get; set; }
    public int CompletionRate { get; set; } // Percentage e.g. 98
    public int YearsActive { get; set; }
    public int TotalDeliveries { get; set; }
    
    // Personal Info from Images
    public string? IdentityNumber { get; set; } // CCCD/CMND
    public string? DriverLicenseNumber { get; set; } // Giấy phép lái xe
}

public class DriverIncomeSummaryDto
{
    public string PeriodLabel { get; set; } = null!; // e.g. "Tháng 10, 2023"
    public decimal TotalIncome { get; set; }
    public double GrowthPercentage { get; set; } // e.g. 12.0
    public int CompletedOrders { get; set; }
    public double TotalDistanceKm { get; set; }
    public decimal MonthlyBonus { get; set; }
    public List<IncomeChartPointDto> ChartData { get; set; } = new();
}

public class IncomeChartPointDto
{
    public string Label { get; set; } = null!; // e.g. "Tuần 1"
    public decimal Value { get; set; }
}

public class DriverIncomeHistoryGroupDto
{
    public string DateLabel { get; set; } = null!; // Today, Yesterday, or Date
    public string DateValue { get; set; } = null!; // e.g. "20/10"
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<DriverIncomeItemDto> Items { get; set; } = new();
}

public class DriverIncomeItemDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string MerchantName { get; set; } = null!;
    public string Time { get; set; } = null!; // e.g. "10:30 AM"
    public double DistanceKm { get; set; }
    public decimal Amount { get; set; }
}

public class DriverDocumentDto
{
    public string Type { get; set; } = null!; // CI, DL, CriminalRecord, Insurance, Registration
    public string Name { get; set; } = null!; // Readable name
    public string Status { get; set; } = null!; // Approved, Pending, Expired, ActionRequired
    public DateTime? ExpiryDate { get; set; }
    public string? DocumentUrl { get; set; }
}

public class AvailableOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string RestaurantName { get; set; } = null!;
    public string RestaurantAddress { get; set; } = null!;
    public string DeliveryAddress { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public double Distance { get; set; } // Calculated
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ActiveDeliveryDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int Status { get; set; }
    public string StatusText { get; set; } = null!;
    
    // Restaurant Info
    public string RestaurantName { get; set; } = null!;
    public string RestaurantAddress { get; set; } = null!;
    public double RestaurantLat { get; set; }
    public double RestaurantLng { get; set; }
    
    // Customer Info
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string DeliveryAddress { get; set; } = null!;
    public double DeliveryLat { get; set; }
    public double DeliveryLng { get; set; }
    
    public decimal TotalAmount { get; set; }
    public int PaymentMethod { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
}

public class UpdateOrderStatusRequest
{
    public int NewStatus { get; set; }
}

public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class DriverWalletDto
{
    public decimal Balance { get; set; }
    public decimal TodayEarnings { get; set; }
    public decimal WeekEarnings { get; set; }
    public decimal MonthEarnings { get; set; }
    public decimal PendingWithdrawal { get; set; }
    public List<DriverTransactionDto> Transactions { get; set; } = new();
}

public class DriverTransactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!; // earning, withdrawal, bonus, fee
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
