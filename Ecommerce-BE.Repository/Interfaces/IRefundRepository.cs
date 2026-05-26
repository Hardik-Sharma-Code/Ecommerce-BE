using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(int id);
    Task<IEnumerable<Refund>> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<Refund>> GetByUserIdAsync(string userId);
    Task<(IEnumerable<Refund> Refunds, int TotalCount)> GetAllAsync(int page, int pageSize, RefundStatus? status = null);
    Task<decimal> GetTotalRefundedAsync(int orderId);
    Task<Refund> CreateAsync(Refund refund);
    Task<Refund> UpdateAsync(Refund refund);
}
