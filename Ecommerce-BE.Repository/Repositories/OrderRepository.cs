using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) => _context = context;

    private IQueryable<Order> FullQuery() =>
        _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .Include(o => o.Refunds);

    public async Task<Order?> GetByIdAsync(int id) =>
        await FullQuery().FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetByIdForUserAsync(int id, string userId) =>
        await FullQuery().FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber) =>
        await FullQuery().FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetByUserIdAsync(
        string userId, int page, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAt);

        var total = await query.CountAsync();
        var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (orders, total);
    }

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllAsync(
        int page, int pageSize, OrderStatus? status = null)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        query = query.OrderByDescending(o => o.PlacedAt);
        var total = await query.CountAsync();
        var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (orders, total);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
}
