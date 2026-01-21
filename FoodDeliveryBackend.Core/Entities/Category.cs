namespace FoodDeliveryBackend.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameSecondary { get; set; }
    public string? ImageUrl { get; set; } // Maps to 'image' in frontend
    public string? IconUrl { get; set; }
    public string? BackgroundColor { get; set; }
    public int DisplayOrder { get; set; }
}
