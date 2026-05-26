using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Order;

public class PlaceOrderDto
{
    [Required]
    public int AddressId { get; set; }

    [Required]
    public string ShippingMethod { get; set; } = "Standard"; // Standard | Express | Overnight

    public string? CouponCode { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
