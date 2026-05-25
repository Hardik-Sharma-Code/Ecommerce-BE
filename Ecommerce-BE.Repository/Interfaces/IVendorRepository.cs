using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IVendorRepository
{
    Task<VendorProfile?> GetByUserIdAsync(string userId);
    Task<VendorProfile> CreateAsync(VendorProfile profile);
    Task<VendorProfile> UpdateAsync(VendorProfile profile);
}
