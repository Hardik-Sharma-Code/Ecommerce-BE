using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Coupon;

public class UpdateCouponDto
{
    [MaxLength(500)]
    public string? Description { get; set; }

    public DiscountType? DiscountType { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinOrderAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool? IsActive { get; set; }
}
