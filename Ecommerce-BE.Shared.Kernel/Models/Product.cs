namespace Ecommerce_BE.Shared.Kernel.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string VendorId { get; set; } = string.Empty;
    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public string? Tags { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
