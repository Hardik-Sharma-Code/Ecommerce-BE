using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IFileRepository
{
    Task<FileRecord?> GetByIdAsync(int id);
    Task<List<FileRecord>> GetByEntityAsync(string entityType, string entityId);
    Task AddAsync(FileRecord record);
    Task DeleteAsync(FileRecord record);
    Task SaveAsync();
}
