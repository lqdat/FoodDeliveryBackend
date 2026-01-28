using FoodDeliveryBackend.Core.Entities.Approval;
using FoodDeliveryBackend.Core.Entities.ChainOwner;
using FoodDeliveryBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Account service implementation for ChainOwner hierarchy.
/// Handles registration, authentication, and entity creation with approval workflow.
/// </summary>
public class AccountService : IAccountService
{
    private readonly FoodDeliveryDbContext _context;
    private readonly IApprovalService _approvalService;

    public AccountService(FoodDeliveryDbContext context, IApprovalService approvalService)
    {
        _context = context;
        _approvalService = approvalService;
    }

    public async Task<ChainOwnerAccount> RegisterChainOwnerAsync(
        string email,
        string password,
        string fullName,
        string? phoneNumber,
        string businessName,
        string? businessRegistrationNumber,
        string regionCode)
    {
        // Check if email already exists
        var existing = await _context.ChainOwnerAccounts
            .FirstOrDefaultAsync(c => c.Email == email);
        if (existing != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        var chainOwner = new ChainOwnerAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            PhoneNumber = phoneNumber,
            BusinessName = businessName,
            BusinessRegistrationNumber = businessRegistrationNumber,
            RegionCode = regionCode.ToUpperInvariant(),
            Status = AccountStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.ChainOwnerAccounts.Add(chainOwner);
            await _context.SaveChangesAsync();

            // Auto-submit for approval
            await _approvalService.SubmitForApprovalAsync(
                EntityType.ChainOwner,
                chainOwner.Id,
                chainOwner.RegionCode,
                chainOwner.Id,
                "ChainOwner");

            await transaction.CommitAsync();
            return chainOwner;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ChainOwnerAccount?> GetChainOwnerByIdAsync(Guid id)
    {
        return await _context.ChainOwnerAccounts
            .Include(c => c.Stores)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ChainOwnerAccount?> AuthenticateChainOwnerAsync(string email, string password)
    {
        var chainOwner = await _context.ChainOwnerAccounts
            .FirstOrDefaultAsync(c => c.Email == email);

        if (chainOwner == null || !BCrypt.Net.BCrypt.Verify(password, chainOwner.PasswordHash))
        {
            return null;
        }

        chainOwner.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return chainOwner;
    }

    public async Task<StoreAccount> CreateStoreAsync(
        Guid chainOwnerId,
        string storeName,
        string? description,
        string address,
        double latitude,
        double longitude,
        string? phoneNumber,
        string? regionCode = null)
    {
        var chainOwner = await _context.ChainOwnerAccounts.FindAsync(chainOwnerId);
        if (chainOwner == null)
        {
            throw new KeyNotFoundException("ChainOwner not found");
        }

        // Business rule: ChainOwner must be Active to create stores
        if (chainOwner.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException("ChainOwner must be Active to create stores");
        }

        var store = new StoreAccount
        {
            Id = Guid.NewGuid(),
            ChainOwnerId = chainOwnerId,
            StoreName = storeName,
            Description = description,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            PhoneNumber = phoneNumber,
            RegionCode = regionCode?.ToUpperInvariant() ?? chainOwner.RegionCode,
            Status = AccountStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.StoreAccounts.Add(store);
            await _context.SaveChangesAsync();

            // Auto-submit for approval
            await _approvalService.SubmitForApprovalAsync(
                EntityType.StoreAccount,
                store.Id,
                store.RegionCode,
                chainOwnerId,
                "ChainOwner");

            await transaction.CommitAsync();
            return store;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<StoreAccount>> GetStoresByChainOwnerAsync(Guid chainOwnerId)
    {
        return await _context.StoreAccounts
            .Include(s => s.Managers)
            .Include(s => s.Foods)
            .Where(s => s.ChainOwnerId == chainOwnerId)
            .ToListAsync();
    }

    public async Task<StoreManager> CreateManagerAsync(
        Guid chainOwnerId,
        Guid storeAccountId,
        string email,
        string password,
        string fullName,
        string? phoneNumber)
    {
        var store = await _context.StoreAccounts
            .Include(s => s.ChainOwner)
            .FirstOrDefaultAsync(s => s.Id == storeAccountId);

        if (store == null)
        {
            throw new KeyNotFoundException("Store not found");
        }

        // Verify chain owner ownership
        if (store.ChainOwnerId != chainOwnerId)
        {
            throw new UnauthorizedAccessException("Store does not belong to this ChainOwner");
        }

        // Business rule: Store must be Active to create managers
        if (store.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException("Store must be Active to create managers");
        }

        // Check email uniqueness
        var existingManager = await _context.StoreManagers
            .FirstOrDefaultAsync(m => m.Email == email);
        if (existingManager != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        var manager = new StoreManager
        {
            Id = Guid.NewGuid(),
            StoreAccountId = storeAccountId,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Status = AccountStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.StoreManagers.Add(manager);
            await _context.SaveChangesAsync();

            // Auto-submit for approval
            await _approvalService.SubmitForApprovalAsync(
                EntityType.StoreManager,
                manager.Id,
                store.RegionCode,
                chainOwnerId,
                "ChainOwner");

            await transaction.CommitAsync();
            return manager;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StoreManager?> AuthenticateManagerAsync(string email, string password)
    {
        var manager = await _context.StoreManagers
            .Include(m => m.Store)
            .FirstOrDefaultAsync(m => m.Email == email);

        if (manager == null || !BCrypt.Net.BCrypt.Verify(password, manager.PasswordHash))
        {
            return null;
        }

        manager.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return manager;
    }
}
