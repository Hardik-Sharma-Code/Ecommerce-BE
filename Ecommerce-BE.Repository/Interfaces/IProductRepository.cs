using Ecommerce_BE.Shared.Kernel.DTOs.Search;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, bool includeImages = true);
    Task<Product?> GetBySlugAsync(string slug);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int page, int pageSize, bool activeOnly = true);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetByCategoryAsync(int categoryId, int page, int pageSize);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetByVendorAsync(string vendorId, int page, int pageSize);
    Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(ProductSearchRequestDto request);
    Task<IEnumerable<string>> GetNameSuggestionsAsync(string query, int limit = 10);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    Task<int> GetCountByCategoryAsync(int categoryId);
    Task DecrementStockAsync(int productId, int quantity);
}
