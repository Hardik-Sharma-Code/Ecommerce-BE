using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.AuditLog;

public class AuditLogFilterDto
{
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Fix #6: [Range] ensures page=0 is rejected at model-binding time before reaching the repository
    [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;
}
