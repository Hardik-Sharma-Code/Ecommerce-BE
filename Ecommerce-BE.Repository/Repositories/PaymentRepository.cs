using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context) => _context = context;

    public async Task<Payment?> GetByIdAsync(int id) =>
        await _context.Payments.FindAsync(id);

    public async Task<Payment?> GetByOrderIdAsync(int orderId) =>
        await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task<Payment?> GetByGatewayOrderIdAsync(string gatewayOrderId) =>
        await _context.Payments.FirstOrDefaultAsync(p => p.GatewayOrderId == gatewayOrderId);

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        payment.UpdatedAt = DateTime.UtcNow;
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
}
