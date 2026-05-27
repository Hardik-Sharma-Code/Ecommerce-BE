using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Payment;

public class InitiatePaymentDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public PaymentGatewayType Gateway { get; set; }

    public string Currency { get; set; } = "INR";
}
