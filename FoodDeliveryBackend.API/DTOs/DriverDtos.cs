namespace FoodDeliveryBackend.API.DTOs;

public class DriverProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string VehicleType { get; set; } = null!;
    public string? LicensePlate { get; set; }
    public bool IsOnline { get; set; }
    public bool IsVerified { get; set; }
    public decimal WalletBalance { get; set; }
    public double Rating { get; set; }
    public int TotalDeliveries { get; set; }
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

public class DriverEarningDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DriverWalletDto
{
    public decimal Balance { get; set; }
    public decimal TodayEarnings { get; set; }
    public decimal WeekEarnings { get; set; }
    public List<DriverEarningDto> RecentEarnings { get; set; } = new();
}

public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
