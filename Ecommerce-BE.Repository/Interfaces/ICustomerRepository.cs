using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerProfile?> GetByUserIdAsync(string userId);
    Task<CustomerProfile> CreateAsync(CustomerProfile profile);
    Task<CustomerProfile> UpdateAsync(CustomerProfile profile);
}
