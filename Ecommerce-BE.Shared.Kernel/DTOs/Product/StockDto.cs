using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Product;

public class UpdateStockDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue)]
    public int? LowStockThreshold { get; set; }
}

public class StockInfoDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
}
