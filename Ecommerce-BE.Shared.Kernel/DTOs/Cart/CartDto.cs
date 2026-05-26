namespace Ecommerce_BE.Shared.Kernel.DTOs.Cart;

public class CartDto
{
    public IEnumerable<CartItemDto> Items { get; set; } = Enumerable.Empty<CartItemDto>();
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
}
