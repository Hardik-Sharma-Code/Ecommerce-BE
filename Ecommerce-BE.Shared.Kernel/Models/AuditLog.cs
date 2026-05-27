namespace Ecommerce_BE.Shared.Kernel.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int? HttpStatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}
