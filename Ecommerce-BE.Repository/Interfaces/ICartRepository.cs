using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);
    Task<Cart> GetOrCreateAsync(string userId);
    Task<CartItem?> GetItemByIdAsync(int itemId);
    Task<CartItem?> GetItemAsync(int cartId, int productId);
    Task AddItemAsync(CartItem item);
    Task UpdateItemAsync(CartItem item);
    Task RemoveItemAsync(CartItem item);
    Task ClearItemsAsync(int cartId);
    Task SaveAsync();
}
