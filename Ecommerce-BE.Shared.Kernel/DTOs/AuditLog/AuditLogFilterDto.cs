namespace Ecommerce_BE.Shared.Kernel.DTOs.AuditLog;

public class AuditLogFilterDto
{
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
