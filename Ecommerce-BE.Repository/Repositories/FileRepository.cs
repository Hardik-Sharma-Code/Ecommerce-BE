using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _context;

    public FileRepository(ApplicationDbContext context) => _context = context;

    public async Task<FileRecord?> GetByIdAsync(int id) =>
        await _context.FileRecords.FirstOrDefaultAsync(f => f.Id == id);

    public async Task<List<FileRecord>> GetByEntityAsync(string entityType, string entityId) =>
        await _context.FileRecords
            .Where(f => f.EntityType == entityType && f.EntityId == entityId)
            .AsNoTracking()
            .ToListAsync();

    public async Task AddAsync(FileRecord record) => await _context.FileRecords.AddAsync(record);
    public Task DeleteAsync(FileRecord record) { _context.FileRecords.Remove(record); return Task.CompletedTask; }
    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
