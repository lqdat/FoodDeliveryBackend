using FoodDeliveryBackend.Core.Entities.Approval;
using FoodDeliveryBackend.Core.Entities.ChainOwner;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Food service implementation with approval workflow integration.
/// </summary>
public class FoodService : IFoodService
{
    private readonly FoodDeliveryDbContext _context;
    private readonly IApprovalService _approvalService;

    public FoodService(FoodDeliveryDbContext context, IApprovalService approvalService)
    {
        _context = context;
        _approvalService = approvalService;
    }

    public async Task<Food> CreateFoodAsync(
        Guid storeAccountId,
        Guid creatorId,
        string creatorAccountType,
        string name,
        string? description,
        decimal price,
        decimal? originalPrice,
        string? imageUrl,
        string? category,
        int displayOrder = 0)
    {
        var store = await _context.StoreAccounts.FindAsync(storeAccountId);
        if (store == null)
        {
            throw new KeyNotFoundException("Store not found");
        }

        // Note: Food can be created even if store is not Active (as draft)
        // But it can only be Published if store is Active

        var food = new Food
        {
            Id = Guid.NewGuid(),
            StoreAccountId = storeAccountId,
            Name = name,
            Description = description,
            Price = price,
            OriginalPrice = originalPrice,
            ImageUrl = imageUrl,
            Category = category,
            DisplayOrder = displayOrder,
            Status = FoodStatus.PendingApproval,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            // Auto-submit for approval
            await _approvalService.SubmitForApprovalAsync(
                EntityType.Food,
                food.Id,
                store.RegionCode,
                creatorId,
                creatorAccountType);

            await transaction.CommitAsync();
            return food;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Food?> GetFoodByIdAsync(Guid id)
    {
        return await _context.Foods
            .Include(f => f.Store)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Food>> GetFoodsByStoreAsync(Guid storeAccountId)
    {
        return await _context.Foods
            .Where(f => f.StoreAccountId == storeAccountId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Food> UpdateFoodAsync(
        Guid foodId,
        Guid updaterId,
        string? name = null,
        string? description = null,
        decimal? price = null,
        decimal? originalPrice = null,
        string? imageUrl = null,
        string? category = null,
        int? displayOrder = null,
        bool? isAvailable = null)
    {
        var food = await _context.Foods.FindAsync(foodId);
        if (food == null)
        {
            throw new KeyNotFoundException("Food not found");
        }

        if (name != null) food.Name = name;
        if (description != null) food.Description = description;
        if (price.HasValue) food.Price = price.Value;
        if (originalPrice.HasValue) food.OriginalPrice = originalPrice.Value;
        if (imageUrl != null) food.ImageUrl = imageUrl;
        if (category != null) food.Category = category;
        if (displayOrder.HasValue) food.DisplayOrder = displayOrder.Value;
        if (isAvailable.HasValue) food.IsAvailable = isAvailable.Value;
        
        food.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return food;
    }

    public async Task DeleteFoodAsync(Guid foodId, Guid deleterId)
    {
        var food = await _context.Foods.FindAsync(foodId);
        if (food == null)
        {
            throw new KeyNotFoundException("Food not found");
        }

        _context.Foods.Remove(food);
        await _context.SaveChangesAsync();
    }
}
