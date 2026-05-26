using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.File;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Ecommerce_BE.Services.Implementations;

public class FileService : IFileService
{
    private readonly IFileRepository _files;
    private readonly FileUploadSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileService(IFileRepository files, IOptions<FileUploadSettings> settings, IHttpContextAccessor httpContextAccessor)
    {
        _files = files;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<FileRecordDto>> UploadAsync(IFormFile file, string uploadedBy, string? entityType = null, string? entityId = null)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<FileRecordDto>.Fail("No file provided");

        if (file.Length > _settings.MaxFileSizeBytes)
            return ApiResponse<FileRecordDto>.Fail($"File size exceeds the maximum allowed size of {_settings.MaxFileSizeBytes / (1024 * 1024)} MB");

        var allowed = _settings.AllowedImageTypes.Concat(_settings.AllowedDocumentTypes).ToArray();
        if (!allowed.Contains(file.ContentType.ToLower()))
            return ApiResponse<FileRecordDto>.Fail("File type not allowed");

        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid():N}{ext}";
        var relativePath = Path.Combine(_settings.UploadPath, storedName).Replace("\\", "/");
        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        using (var stream = new FileStream(physicalPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var request = _httpContextAccessor.HttpContext?.Request;
        var publicUrl = request != null
            ? $"{request.Scheme}://{request.Host}/{relativePath}"
            : $"/{relativePath}";

        var record = new FileRecord
        {
            OriginalName = file.FileName,
            StoredName = storedName,
            RelativePath = relativePath,
            PublicUrl = publicUrl,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedBy = uploadedBy,
            EntityType = entityType,
            EntityId = entityId,
            UploadedAt = DateTime.UtcNow
        };

        await _files.AddAsync(record);
        await _files.SaveAsync();

        return ApiResponse<FileRecordDto>.Ok(MapToDto(record));
    }

    public async Task<ApiResponse<List<FileRecordDto>>> GetByEntityAsync(string entityType, string entityId)
    {
        var records = await _files.GetByEntityAsync(entityType, entityId);
        return ApiResponse<List<FileRecordDto>>.Ok(records.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse> DeleteAsync(int id, string requestedBy, bool isAdmin)
    {
        var record = await _files.GetByIdAsync(id);
        if (record == null) return ApiResponse.Fail("File not found");
        if (!isAdmin && record.UploadedBy != requestedBy) return ApiResponse.Fail("Access denied");

        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", record.RelativePath);
        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        await _files.DeleteAsync(record);
        await _files.SaveAsync();

        return ApiResponse.Ok("File deleted");
    }

    private static FileRecordDto MapToDto(FileRecord f) => new()
    {
        Id = f.Id,
        OriginalName = f.OriginalName,
        PublicUrl = f.PublicUrl,
        ContentType = f.ContentType,
        FileSize = f.FileSize,
        EntityType = f.EntityType,
        EntityId = f.EntityId,
        UploadedAt = f.UploadedAt
    };
}
