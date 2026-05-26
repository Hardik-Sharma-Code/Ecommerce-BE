using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Payment;

public class PaymentInitiatedDto
{
    public int OrderId { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }      // Stripe PaymentIntent client_secret
    public string PublicKey { get; set; } = string.Empty; // Razorpay key_id / Stripe publishable key
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentGatewayType Gateway { get; set; }
    public string GatewayName => Gateway.ToString();
}
