namespace Ecommerce_BE.Shared.Kernel.DTOs.File;

public class FileRecordDto
{
    public int Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime UploadedAt { get; set; }
}
