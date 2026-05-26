using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IAddressRepository
{
    Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(string userId);
    Task<CustomerAddress?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id, string userId);
    Task<bool> HasDefaultAsync(string userId);
    Task ClearDefaultAsync(string userId);
    Task AddAsync(CustomerAddress address);
    Task UpdateAsync(CustomerAddress address);
    Task DeleteAsync(CustomerAddress address);
    Task SaveAsync();
}
