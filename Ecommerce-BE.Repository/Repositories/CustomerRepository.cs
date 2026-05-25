using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerProfile?> GetByUserIdAsync(string userId) =>
        await _context.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<CustomerProfile> CreateAsync(CustomerProfile profile)
    {
        _context.CustomerProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<CustomerProfile> UpdateAsync(CustomerProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.CustomerProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }
}
