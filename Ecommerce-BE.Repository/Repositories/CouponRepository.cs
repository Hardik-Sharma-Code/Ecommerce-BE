using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context) => _context = context;

    public async Task<Coupon?> GetByIdAsync(int id) =>
        await _context.Coupons.FindAsync(id);

    public async Task<Coupon?> GetByCodeAsync(string code) =>
        await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper());

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Coupons.Where(c => c.Code == code.ToUpper());
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<(IEnumerable<Coupon> Coupons, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var query = _context.Coupons.OrderByDescending(c => c.CreatedAt);
        var total = await query.CountAsync();
        var coupons = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (coupons, total);
    }

    public async Task AddAsync(Coupon coupon)
    {
        _context.Coupons.Add(coupon);
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Coupon coupon)
    {
        _context.Coupons.Remove(coupon);
        await Task.CompletedTask;
    }

    public async Task IncrementUsageAsync(int couponId)
    {
        await _context.Coupons
            .Where(c => c.Id == couponId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsageCount, c => c.UsageCount + 1));
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
