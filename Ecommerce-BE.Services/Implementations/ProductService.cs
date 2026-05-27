using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Product;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ProductListDto>>> GetAllAsync(int page, int pageSize)
    {
        var (products, total) = await _productRepository.GetAllAsync(page, pageSize);
        return ApiResponse<PagedResult<ProductListDto>>.Ok(ToPagedResult(products, total, page, pageSize));
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return ApiResponse<ProductDto>.Fail("Product not found.");

        return ApiResponse<ProductDto>.Ok(await MapToDtoAsync(product));
    }

    public async Task<ApiResponse<ProductDto>> GetBySlugAsync(string slug)
    {
        var product = await _productRepository.GetBySlugAsync(slug);
        if (product is null)
            return ApiResponse<ProductDto>.Fail("Product not found.");

        return ApiResponse<ProductDto>.Ok(await MapToDtoAsync(product));
    }

    public async Task<ApiResponse<PagedResult<ProductListDto>>> GetByCategoryAsync(
        int categoryId, int page, int pageSize)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category is null)
            return ApiResponse<PagedResult<ProductListDto>>.Fail("Category not found.");

        var (products, total) = await _productRepository.GetByCategoryAsync(categoryId, page, pageSize);
        return ApiResponse<PagedResult<ProductListDto>>.Ok(ToPagedResult(products, total, page, pageSize));
    }

    public async Task<ApiResponse<PagedResult<ProductListDto>>> GetByVendorAsync(
        string vendorId, int page, int pageSize)
    {
        var (products, total) = await _productRepository.GetByVendorAsync(vendorId, page, pageSize);
        return ApiResponse<PagedResult<ProductListDto>>.Ok(ToPagedResult(products, total, page, pageSize));
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(string vendorId, CreateProductDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            return ApiResponse<ProductDto>.Fail("Category not found.");

        if (await _productRepository.SkuExistsAsync(dto.SKU))
            return ApiResponse<ProductDto>.Fail($"A product with SKU '{dto.SKU}' already exists.");

        var slug = await GenerateUniqueSlugAsync(dto.Name);

        var product = new Product
        {
            Name = dto.Name,
            Slug = slug,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            CompareAtPrice = dto.CompareAtPrice,
            CategoryId = dto.CategoryId,
            VendorId = vendorId,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            IsFeatured = dto.IsFeatured,
            Tags = dto.Tags,
            IsActive = true,
            Images = dto.Images.Select((img, idx) => new ProductImage
            {
                ImageUrl = img.ImageUrl,
                AltText = img.AltText,
                IsPrimary = img.IsPrimary || idx == 0,
                SortOrder = img.SortOrder > 0 ? img.SortOrder : idx
            }).ToList()
        };

        var created = await _productRepository.CreateAsync(product);
        _logger.LogInformation("Product created: {Name} (Id: {Id}) by vendor {VendorId}", created.Name, created.Id, vendorId);
        return ApiResponse<ProductDto>.Ok(await MapToDtoAsync(created), "Product created successfully.");
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(
        int id, string userId, string userRole, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return ApiResponse<ProductDto>.Fail("Product not found.");

        if (userRole == Roles.Vendor && product.VendorId != userId)
            return ApiResponse<ProductDto>.Fail("You are not authorized to update this product.");

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            return ApiResponse<ProductDto>.Fail("Category not found.");

        if (await _productRepository.SkuExistsAsync(dto.SKU, excludeId: id))
            return ApiResponse<ProductDto>.Fail($"A product with SKU '{dto.SKU}' already exists.");

        if (!product.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
            product.Slug = await GenerateUniqueSlugAsync(dto.Name, excludeId: id);

        // Replace images
        product.Images.Clear();
        var images = dto.Images.Select((img, idx) => new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = img.ImageUrl,
            AltText = img.AltText,
            IsPrimary = img.IsPrimary || idx == 0,
            SortOrder = img.SortOrder > 0 ? img.SortOrder : idx
        }).ToList();

        product.Name = dto.Name;
        product.ShortDescription = dto.ShortDescription;
        product.Description = dto.Description;
        product.SKU = dto.SKU;
        product.Price = dto.Price;
        product.CompareAtPrice = dto.CompareAtPrice;
        product.CategoryId = dto.CategoryId;
        product.LowStockThreshold = dto.LowStockThreshold;
        product.IsActive = dto.IsActive;
        product.IsFeatured = dto.IsFeatured;
        product.Tags = dto.Tags;
        product.Images = images;

        var updated = await _productRepository.UpdateAsync(product);
        _logger.LogInformation("Product updated: {Name} (Id: {Id})", updated.Name, updated.Id);
        return ApiResponse<ProductDto>.Ok(await MapToDtoAsync(updated));
    }

    public async Task<ApiResponse> DeleteAsync(int id, string userId, string userRole)
    {
        var product = await _productRepository.GetByIdAsync(id, includeImages: false);
        if (product is null)
            return ApiResponse.Fail("Product not found.");

        if (userRole == Roles.Vendor && product.VendorId != userId)
            return ApiResponse.Fail("You are not authorized to delete this product.");

        await _productRepository.DeleteAsync(product);
        _logger.LogInformation("Product deleted: Id {Id}", id);
        return ApiResponse.Ok("Product deleted successfully.");
    }

    public async Task<ApiResponse<StockInfoDto>> GetStockAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id, includeImages: false);
        if (product is null)
            return ApiResponse<StockInfoDto>.Fail("Product not found.");

        return ApiResponse<StockInfoDto>.Ok(MapToStockDto(product));
    }

    public async Task<ApiResponse<StockInfoDto>> UpdateStockAsync(
        int id, string userId, string userRole, UpdateStockDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id, includeImages: false);
        if (product is null)
            return ApiResponse<StockInfoDto>.Fail("Product not found.");

        if (userRole == Roles.Vendor && product.VendorId != userId)
            return ApiResponse<StockInfoDto>.Fail("You are not authorized to update stock for this product.");

        product.StockQuantity = dto.StockQuantity;
        if (dto.LowStockThreshold.HasValue)
            product.LowStockThreshold = dto.LowStockThreshold.Value;

        await _productRepository.UpdateAsync(product);
        _logger.LogInformation("Stock updated for product {Id}: qty={Qty}", id, dto.StockQuantity);
        return ApiResponse<StockInfoDto>.Ok(MapToStockDto(product));
    }

    // -------------------------------------------------------------------------
    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId = null)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var counter = 1;

        while (await _productRepository.SlugExistsAsync(slug, excludeId))
            slug = $"{baseSlug}-{counter++}";

        return slug;
    }

    private async Task<ProductDto> MapToDtoAsync(Product p)
    {
        var vendor = await _userManager.FindByIdAsync(p.VendorId);
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            ShortDescription = p.ShortDescription,
            Description = p.Description,
            SKU = p.SKU,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            VendorId = p.VendorId,
            VendorName = vendor is not null ? $"{vendor.FirstName} {vendor.LastName}" : string.Empty,
            StockQuantity = p.StockQuantity,
            LowStockThreshold = p.LowStockThreshold,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            IsInStock = p.StockQuantity > 0,
            IsLowStock = p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold,
            Tags = p.Tags,
            Images = p.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder
            }).ToList(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }

    private static ProductListDto MapToListDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        ShortDescription = p.ShortDescription,
        Price = p.Price,
        CompareAtPrice = p.CompareAtPrice,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                       ?? p.Images.FirstOrDefault()?.ImageUrl,
        StockQuantity = p.StockQuantity,
        IsInStock = p.StockQuantity > 0,
        IsLowStock = p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold,
        IsFeatured = p.IsFeatured
    };

    private static StockInfoDto MapToStockDto(Product p) => new()
    {
        ProductId = p.Id,
        ProductName = p.Name,
        SKU = p.SKU,
        StockQuantity = p.StockQuantity,
        LowStockThreshold = p.LowStockThreshold,
        IsInStock = p.StockQuantity > 0,
        IsLowStock = p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold
    };

    private static PagedResult<ProductListDto> ToPagedResult(
        IEnumerable<Product> products, int total, int page, int pageSize) => new()
    {
        Items = products.Select(MapToListDto),
        TotalCount = total,
        Page = page,
        PageSize = pageSize
    };
}
