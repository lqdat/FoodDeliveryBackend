using FoodDeliveryBackend.Core.Entities.ChainOwner;

namespace FoodDeliveryBackend.API.Services;

/// <summary>
/// Service interface for ChainOwner account hierarchy management.
/// </summary>
public interface IAccountService
{
    // Chain Owner operations
    Task<ChainOwnerAccount> RegisterChainOwnerAsync(
        string email, 
        string password, 
        string fullName, 
        string? phoneNumber,
        string businessName,
        string? businessRegistrationNumber,
        string regionCode);

    Task<ChainOwnerAccount?> GetChainOwnerByIdAsync(Guid id);
    Task<ChainOwnerAccount?> AuthenticateChainOwnerAsync(string email, string password);

    // Store operations
    Task<StoreAccount> CreateStoreAsync(
        Guid chainOwnerId,
        string storeName,
        string? description,
        string address,
        double latitude,
        double longitude,
        string? phoneNumber,
        string? regionCode = null);

    Task<IEnumerable<StoreAccount>> GetStoresByChainOwnerAsync(Guid chainOwnerId);

    // Store Manager operations
    Task<StoreManager> CreateManagerAsync(
        Guid chainOwnerId,
        Guid storeAccountId,
        string email,
        string password,
        string fullName,
        string? phoneNumber);

    Task<StoreManager?> AuthenticateManagerAsync(string email, string password);
}
