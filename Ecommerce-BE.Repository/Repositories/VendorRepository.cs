using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly ApplicationDbContext _context;

    public VendorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VendorProfile?> GetByUserIdAsync(string userId) =>
        await _context.VendorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId);

    public async Task<VendorProfile> CreateAsync(VendorProfile profile)
    {
        _context.VendorProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<VendorProfile> UpdateAsync(VendorProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.VendorProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }
}
