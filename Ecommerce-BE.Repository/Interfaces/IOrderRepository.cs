using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdForUserAsync(int id, string userId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetByUserIdAsync(string userId, int page, int pageSize);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllAsync(int page, int pageSize, OrderStatus? status = null);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
}
