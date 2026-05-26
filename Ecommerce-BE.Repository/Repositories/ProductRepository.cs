using Ecommerce_BE.Repository.Data;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Shared.Kernel.DTOs.Search;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_BE.Repository.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id, bool includeImages = true)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsNoTracking();

        if (includeImages)
            query = query.Include(p => p.Images.OrderBy(i => i.SortOrder));

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        int page, int pageSize, bool activeOnly = true)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsNoTracking();

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetByCategoryAsync(
        int categoryId, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive);

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetByVendorAsync(
        string vendorId, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId);

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(
        ProductSearchRequestDto request)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)) ||
                (p.Tags != null && p.Tags.ToLower().Contains(term)));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.VendorId))
            query = query.Where(p => p.VendorId == request.VendorId);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.InStockOnly == true)
            query = query.Where(p => p.StockQuantity > 0);

        if (request.FeaturedOnly == true)
            query = query.Where(p => p.IsFeatured);

        var totalCount = await query.CountAsync();

        query = request.SortBy switch
        {
            "price-asc"  => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "name-asc"   => query.OrderBy(p => p.Name),
            "name-desc"  => query.OrderByDescending(p => p.Name),
            "featured"   => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt),
            _            => query.OrderByDescending(p => p.CreatedAt)
        };

        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<IEnumerable<string>> GetNameSuggestionsAsync(string query, int limit = 10) =>
        await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Name.ToLower().Contains(query.ToLower()))
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(p => p.Name)
            .ToListAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _context.Products.Where(p => p.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
    {
        var query = _context.Products.Where(p => p.SKU == sku);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<int> GetCountByCategoryAsync(int categoryId) =>
        await _context.Products.CountAsync(p => p.CategoryId == categoryId && p.IsActive);

    public async Task DecrementStockAsync(int productId, int quantity) =>
        await _context.Products
            .Where(p => p.Id == productId && p.StockQuantity >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity));
}
