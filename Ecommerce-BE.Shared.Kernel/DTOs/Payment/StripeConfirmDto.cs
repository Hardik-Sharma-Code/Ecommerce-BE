using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Payment;

public class StripeConfirmDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;
}
