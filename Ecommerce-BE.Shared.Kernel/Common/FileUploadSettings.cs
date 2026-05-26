namespace Ecommerce_BE.Shared.Kernel.Common;

public class FileUploadSettings
{
    public string UploadPath { get; set; } = "uploads";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
    public string[] AllowedImageTypes { get; set; } = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    public string[] AllowedDocumentTypes { get; set; } = ["application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];
}
