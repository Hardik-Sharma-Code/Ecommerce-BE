using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public PaymentGatewayType Gateway { get; set; }
    public string GatewayName => Gateway.ToString();
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
