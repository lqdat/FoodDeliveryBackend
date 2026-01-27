using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Driver
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? VehicleType { get; set; }

    public string? VehiclePlate { get; set; }
    
    public string? LicensePlate { get; set; }

    public string? VehicleBrand { get; set; }

    public int Status { get; set; }

    public bool IsOnline { get; set; }

    public bool IsVerified { get; set; }

    public decimal WalletBalance { get; set; }

    public double? CurrentLatitude { get; set; }

    public double? CurrentLongitude { get; set; }

    public double Rating { get; set; }

    public int TotalDeliveries { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? IdentityNumber { get; set; }

    public string? DriverLicenseNumber { get; set; }

    public DateTime? LicenseExpiryDate { get; set; }

    public string? InsuranceUrl { get; set; }

    public DateTime? InsuranceExpiryDate { get; set; }

    public string? CriminalRecordUrl { get; set; }

    public DateTime? RegistrationExpiryDate { get; set; }

    public string? DriverLicenseUrl { get; set; }

    public string? IdCardBackUrl { get; set; }

    public string? IdCardFrontUrl { get; set; }

    public bool IsApproved { get; set; }

    public string? RejectionReason { get; set; }

    public string? VehicleRegistrationUrl { get; set; }

    public virtual ICollection<DriverEarning> DriverEarnings { get; set; } = new List<DriverEarning>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
