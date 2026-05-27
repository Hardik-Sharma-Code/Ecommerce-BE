using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Product;

public class UpdateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    public decimal? CompareAtPrice { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    [MaxLength(500)]
    public string? Tags { get; set; }

    public IList<CreateProductImageDto> Images { get; set; } = new List<CreateProductImageDto>();
}
