namespace FoodDeliveryBackend.API.DTOs;

public class AddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!; // Home, Work, etc.
    public string FullAddress { get; set; } = null!;
    public string? Note { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public class CreateAddressRequest
{
    public string Label { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string? Note { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateAddressRequest
{
    public string Label { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public string? Note { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
}
