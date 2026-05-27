using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly ApplicationDbContext _context;

    public AddressRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(string userId) =>
        await _context.CustomerAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<CustomerAddress?> GetByIdAsync(int id) =>
        await _context.CustomerAddresses.FindAsync(id);

    public async Task<bool> ExistsAsync(int id, string userId) =>
        await _context.CustomerAddresses.AnyAsync(a => a.Id == id && a.UserId == userId);

    public async Task<bool> HasDefaultAsync(string userId) =>
        await _context.CustomerAddresses.AnyAsync(a => a.UserId == userId && a.IsDefault);

    public async Task ClearDefaultAsync(string userId)
    {
        var defaults = await _context.CustomerAddresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync();
        foreach (var a in defaults) a.IsDefault = false;
    }

    public async Task AddAsync(CustomerAddress address)
    {
        _context.CustomerAddresses.Add(address);
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(CustomerAddress address)
    {
        _context.CustomerAddresses.Update(address);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(CustomerAddress address)
    {
        _context.CustomerAddresses.Remove(address);
        await Task.CompletedTask;
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
