using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.AuditLog;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context) => _context = context;

    public async Task<AuditLog?> GetByIdAsync(int id) =>
        await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<PagedResult<AuditLog>> GetAllAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
            query = query.Where(a => a.UserId == filter.UserId);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(a => a.Action.Contains(filter.Action));

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.ToDate.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<AuditLog>(items, total, filter.Page, filter.PageSize);
    }

    public async Task AddAsync(AuditLog log) => await _context.AuditLogs.AddAsync(log);
    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
