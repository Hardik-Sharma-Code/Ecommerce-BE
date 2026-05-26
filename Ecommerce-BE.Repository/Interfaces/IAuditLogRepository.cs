using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.AuditLog;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(int id);
    Task<PagedResult<AuditLog>> GetAllAsync(AuditLogFilterDto filter);
    Task AddAsync(AuditLog log);
    Task SaveAsync();
}
