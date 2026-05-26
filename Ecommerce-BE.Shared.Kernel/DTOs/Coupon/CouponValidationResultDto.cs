namespace Ecommerce_BE.Shared.Kernel.DTOs.Coupon;

public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}
