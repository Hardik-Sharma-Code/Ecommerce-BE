using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly ApplicationDbContext _context;

    public WishlistRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId) =>
        await _context.WishlistItems
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();

    public async Task<WishlistItem?> GetItemByIdAsync(int itemId) =>
        await _context.WishlistItems
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(w => w.Id == itemId);

    public async Task<WishlistItem?> GetItemAsync(string userId, int productId) =>
        await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

    public async Task<bool> IsInWishlistAsync(string userId, int productId) =>
        await _context.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == productId);

    public async Task AddAsync(WishlistItem item)
    {
        _context.WishlistItems.Add(item);
        await Task.CompletedTask;
    }

    public async Task RemoveAsync(WishlistItem item)
    {
        _context.WishlistItems.Remove(item);
        await Task.CompletedTask;
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _context.WishlistItems.Where(w => w.UserId == userId).ToListAsync();
        _context.WishlistItems.RemoveRange(items);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
