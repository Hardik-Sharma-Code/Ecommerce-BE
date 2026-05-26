using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context) => _context = context;

    public async Task<Cart?> GetByUserIdAsync(string userId) =>
        await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart> GetOrCreateAsync(string userId)
    {
        var cart = await GetByUserIdAsync(userId);
        if (cart is not null) return cart;

        cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<CartItem?> GetItemByIdAsync(int itemId) =>
        await _context.CartItems
            .Include(i => i.Product)
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task<CartItem?> GetItemAsync(int cartId, int productId) =>
        await _context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);

    public async Task AddItemAsync(CartItem item)
    {
        _context.CartItems.Add(item);
        await Task.CompletedTask;
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        _context.CartItems.Update(item);
        await Task.CompletedTask;
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _context.CartItems.Remove(item);
        await Task.CompletedTask;
    }

    public async Task ClearItemsAsync(int cartId)
    {
        var items = await _context.CartItems.Where(i => i.CartId == cartId).ToListAsync();
        _context.CartItems.RemoveRange(items);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
