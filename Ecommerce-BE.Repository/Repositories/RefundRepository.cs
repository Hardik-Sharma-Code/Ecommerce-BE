using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly ApplicationDbContext _context;

    public RefundRepository(ApplicationDbContext context) => _context = context;

    public async Task<Refund?> GetByIdAsync(int id) =>
        await _context.Refunds
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Refund>> GetByOrderIdAsync(int orderId) =>
        await _context.Refunds
            .Include(r => r.Order)
            .Where(r => r.OrderId == orderId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public async Task<IEnumerable<Refund>> GetByUserIdAsync(string userId) =>
        await _context.Refunds
            .Include(r => r.Order)
            .Where(r => r.Order.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public async Task<(IEnumerable<Refund> Refunds, int TotalCount)> GetAllAsync(
        int page, int pageSize, RefundStatus? status = null)
    {
        var query = _context.Refunds
            .Include(r => r.Order)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        query = query.OrderByDescending(r => r.RequestedAt);
        var total = await query.CountAsync();
        var refunds = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (refunds, total);
    }

    public async Task<decimal> GetTotalRefundedAsync(int orderId) =>
        await _context.Refunds
            .Where(r => r.OrderId == orderId &&
                        (r.Status == RefundStatus.Completed || r.Status == RefundStatus.Processing))
            .SumAsync(r => r.Amount);

    public async Task<Refund> CreateAsync(Refund refund)
    {
        _context.Refunds.Add(refund);
        await _context.SaveChangesAsync();
        return refund;
    }

    public async Task<Refund> UpdateAsync(Refund refund)
    {
        _context.Refunds.Update(refund);
        await _context.SaveChangesAsync();
        return refund;
    }
}
