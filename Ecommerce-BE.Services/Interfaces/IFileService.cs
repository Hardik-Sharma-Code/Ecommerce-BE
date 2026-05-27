using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.File;
using Microsoft.AspNetCore.Http;

namespace Ecommerce_BE.Services.Interfaces;

public interface IFileService
{
    Task<ApiResponse<FileRecordDto>> UploadAsync(IFormFile file, string uploadedBy, string? entityType = null, string? entityId = null);
    Task<ApiResponse<List<FileRecordDto>>> GetByEntityAsync(string entityType, string entityId);
    Task<ApiResponse> DeleteAsync(int id, string requestedBy, bool isAdmin);
}
