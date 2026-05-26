using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Product;
using Ecommerce_BE.Shared.Kernel.DTOs.Search;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class SearchService : ISearchService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IProductRepository productRepository, ILogger<SearchService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ProductListDto>>> SearchAsync(ProductSearchRequestDto request)
    {
        var (products, totalCount) = await _productRepository.SearchAsync(request);

        var result = new PagedResult<ProductListDto>
        {
            Items = products.Select(MapToListDto),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation(
            "Product search: query='{Query}', category={CategoryId}, results={Total}",
            request.Query, request.CategoryId, totalCount);

        return ApiResponse<PagedResult<ProductListDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<string>>> GetSuggestionsAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return ApiResponse<IEnumerable<string>>.Ok(Enumerable.Empty<string>());

        var suggestions = await _productRepository.GetNameSuggestionsAsync(query, limit);
        return ApiResponse<IEnumerable<string>>.Ok(suggestions);
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
}
