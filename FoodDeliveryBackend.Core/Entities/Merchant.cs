using System;
using System.Collections.Generic;

namespace FoodDeliveryBackend.Core.Entities;

public partial class Merchant
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string BusinessName { get; set; } = null!;

    public string? BusinessLicense { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? BusinessLicenseUrl { get; set; }

    public string? FoodSafetyCertUrl { get; set; }

    public string? IdCardFrontUrl { get; set; }

    public bool IsApproved { get; set; }

    public string? RejectionReason { get; set; }

    public virtual ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();

    public virtual User User { get; set; } = null!;
}
