namespace Ecommerce_BE.Shared.Kernel.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Nullable — product may be deleted after purchase, snapshot preserves history
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Product snapshot at time of purchase
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}
