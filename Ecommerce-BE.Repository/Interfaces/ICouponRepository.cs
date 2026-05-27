using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(int id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task<(IEnumerable<Coupon> Coupons, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task AddAsync(Coupon coupon);
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(Coupon coupon);
    Task IncrementUsageAsync(int couponId);
    Task SaveAsync();
}
