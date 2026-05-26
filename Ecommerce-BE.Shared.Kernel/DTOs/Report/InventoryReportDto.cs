namespace Ecommerce_BE.Shared.Kernel.DTOs.Report;

public class InventoryReportDto
{
    public int TotalProducts { get; set; }
    public int InStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public List<InventoryItemDto> Items { get; set; } = new();
}

public class InventoryItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty; // InStock, LowStock, OutOfStock
}
