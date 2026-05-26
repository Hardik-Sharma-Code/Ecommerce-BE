namespace Ecommerce_BE.Shared.Kernel.DTOs.Product;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsFeatured { get; set; }
}
