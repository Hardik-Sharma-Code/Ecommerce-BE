using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment?> GetByGatewayOrderIdAsync(string gatewayOrderId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
}
