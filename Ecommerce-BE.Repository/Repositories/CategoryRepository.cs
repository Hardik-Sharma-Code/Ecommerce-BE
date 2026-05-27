using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id, bool includeSubCategories = false)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking();

        if (includeSubCategories)
            query = query.Include(c => c.SubCategories.Where(s => s.IsActive));

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetBySlugAsync(string slug) =>
        await _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<IEnumerable<Category>> GetAllAsync(bool activeOnly = true)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync() =>
        await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .Include(c => c.SubCategories.Where(s => s.IsActive))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId) =>
        await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == parentId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> HasProductsAsync(int categoryId) =>
        await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
}
