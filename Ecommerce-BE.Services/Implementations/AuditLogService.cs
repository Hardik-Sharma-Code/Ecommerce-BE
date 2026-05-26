using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.AuditLog;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogs;

    public AuditLogService(IAuditLogRepository auditLogs) => _auditLogs = auditLogs;

    public async Task<ApiResponse<PagedResult<AuditLogDto>>> GetAllAsync(AuditLogFilterDto filter)
    {
        var result = await _auditLogs.GetAllAsync(filter);
        return ApiResponse<PagedResult<AuditLogDto>>.Ok(result.Map(MapToDto));
    }

    public async Task<ApiResponse<AuditLogDto>> GetByIdAsync(int id)
    {
        var log = await _auditLogs.GetByIdAsync(id);
        if (log == null) return ApiResponse<AuditLogDto>.Fail("Audit log entry not found");
        return ApiResponse<AuditLogDto>.Ok(MapToDto(log));
    }

    public async Task LogAsync(
        string? userId,
        string? userEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null,
        int? httpStatusCode = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            HttpStatusCode = httpStatusCode,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogs.AddAsync(log);
        await _auditLogs.SaveAsync();
    }

    private static AuditLogDto MapToDto(AuditLog a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        UserEmail = a.UserEmail,
        Action = a.Action,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        Description = a.Description,
        IpAddress = a.IpAddress,
        UserAgent = a.UserAgent,
        HttpStatusCode = a.HttpStatusCode,
        Timestamp = a.Timestamp
    };
}
