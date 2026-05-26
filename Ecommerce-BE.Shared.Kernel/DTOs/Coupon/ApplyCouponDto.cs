using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Coupon;

public class ApplyCouponDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal OrderAmount { get; set; }
}
