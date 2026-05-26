using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Search;

public class ProductSearchRequestDto
{
    public string? Query { get; set; }

    public int? CategoryId { get; set; }

    public string? VendorId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    public bool? InStockOnly { get; set; }

    public bool? FeaturedOnly { get; set; }

    /// <summary>newest | price-asc | price-desc | name-asc | name-desc | featured</summary>
    public string SortBy { get; set; } = "newest";

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
