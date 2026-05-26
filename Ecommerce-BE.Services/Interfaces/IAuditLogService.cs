using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.AuditLog;

namespace Ecommerce_BE.Services.Interfaces;

public interface IAuditLogService
{
    Task<ApiResponse<PagedResult<AuditLogDto>>> GetAllAsync(AuditLogFilterDto filter);
    Task<ApiResponse<AuditLogDto>> GetByIdAsync(int id);
    Task LogAsync(string? userId, string? userEmail, string action, string entityType, string? entityId = null, string? description = null, string? ipAddress = null, string? userAgent = null, int? httpStatusCode = null);
}
