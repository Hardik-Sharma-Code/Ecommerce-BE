namespace Ecommerce_BE.Shared.Kernel.DTOs.Cart;

public class CartItemDto
{
    public int ItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public bool IsInStock { get; set; }
    public int StockQuantity { get; set; }
}
