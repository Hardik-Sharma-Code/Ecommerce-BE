using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IWishlistRepository
{
    Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId);
    Task<WishlistItem?> GetItemByIdAsync(int itemId);
    Task<WishlistItem?> GetItemAsync(string userId, int productId);
    Task<bool> IsInWishlistAsync(string userId, int productId);
    Task AddAsync(WishlistItem item);
    Task RemoveAsync(WishlistItem item);
    Task ClearAsync(string userId);
    Task SaveAsync();
}
