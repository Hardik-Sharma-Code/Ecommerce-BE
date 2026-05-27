using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, bool includeSubCategories = false);
    Task<Category?> GetBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetAllAsync(bool activeOnly = true);
    Task<IEnumerable<Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    Task<bool> HasProductsAsync(int categoryId);
}
