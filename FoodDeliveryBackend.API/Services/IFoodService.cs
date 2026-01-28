using FoodDeliveryBackend.Core.Entities.ChainOwner;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Service interface for Food item management with approval workflow.
/// </summary>
public interface IFoodService
{
    Task<Food> CreateFoodAsync(
        Guid storeAccountId,
        Guid creatorId,
        string creatorAccountType,
        string name,
        string? description,
        decimal price,
        decimal? originalPrice,
        string? imageUrl,
        string? category,
        int displayOrder = 0);

    Task<Food?> GetFoodByIdAsync(Guid id);
    
    Task<IEnumerable<Food>> GetFoodsByStoreAsync(Guid storeAccountId);

    Task<Food> UpdateFoodAsync(
        Guid foodId,
        Guid updaterId,
        string? name = null,
        string? description = null,
        decimal? price = null,
        decimal? originalPrice = null,
        string? imageUrl = null,
        string? category = null,
        int? displayOrder = null,
        bool? isAvailable = null);

    Task DeleteFoodAsync(Guid foodId, Guid deleterId);
}
