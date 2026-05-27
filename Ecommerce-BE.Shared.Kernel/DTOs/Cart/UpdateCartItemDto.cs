using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Cart;

public class UpdateCartItemDto
{
    [Required]
    [Range(0, 100)]
    public int Quantity { get; set; }
}
