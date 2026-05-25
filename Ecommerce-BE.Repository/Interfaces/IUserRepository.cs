using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetByRoleAsync(string role, int page, int pageSize);
    Task UpdateAsync(ApplicationUser user);
    Task DeleteAsync(ApplicationUser user);
}
