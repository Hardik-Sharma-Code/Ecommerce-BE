using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Refund;

public class RequestRefundDto
{
    [Required]
    public int OrderId { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
