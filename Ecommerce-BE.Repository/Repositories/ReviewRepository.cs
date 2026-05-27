using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context) => _context = context;

    public async Task<Review?> GetByIdAsync(int id) =>
        await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Review?> GetByUserAndProductAsync(string userId, int productId) =>
        await _context.Reviews.FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

    public async Task<PagedResult<Review>> GetByProductAsync(int productId, int page, int pageSize, bool approvedOnly = true)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId);

        if (approvedOnly)
            query = query.Where(r => r.IsApproved);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Review>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Review>> GetAllAsync(int page, int pageSize, bool? isApproved = null)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsQueryable();

        if (isApproved.HasValue)
            query = query.Where(r => r.IsApproved == isApproved.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Review>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Review>> GetByUserAsync(string userId, int page, int pageSize)
    {
        var query = _context.Reviews
            .Include(r => r.Product)
            .Where(r => r.UserId == userId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Review>(items, total, page, pageSize);
    }

    public async Task<bool> HasPurchasedProductAsync(string userId, int productId) =>
        await _context.Orders
            .Where(o => o.UserId == userId)
            .AnyAsync(o => o.Items.Any(i => i.ProductId == productId));

    public async Task<(double Average, int Count)> GetRatingSummaryAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        if (reviews.Count == 0) return (0, 0);
        return (reviews.Average(), reviews.Count);
    }

    public async Task<Dictionary<int, int>> GetRatingBreakdownAsync(int productId)
    {
        var breakdown = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.Rating)
            .Select(g => new { Star = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var item in breakdown)
            result[item.Star] = item.Count;
        return result;
    }

    public async Task AddAsync(Review review) => await _context.Reviews.AddAsync(review);
    public Task UpdateAsync(Review review) { _context.Reviews.Update(review); return Task.CompletedTask; }
    public Task DeleteAsync(Review review) { _context.Reviews.Remove(review); return Task.CompletedTask; }
    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
