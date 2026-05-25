using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id) =>
        await _userManager.FindByIdAsync(id);

    public async Task<ApplicationUser?> GetByEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email);

    public async Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var query = _context.Users.AsNoTracking();
        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetByRoleAsync(string role, int page, int pageSize)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        var totalCount = usersInRole.Count;
        var users = usersInRole
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return (users, totalCount);
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteAsync(ApplicationUser user) =>
        await _userManager.DeleteAsync(user);
}
