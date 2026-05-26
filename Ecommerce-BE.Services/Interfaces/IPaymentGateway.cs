using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Interfaces;

public record GatewayOrderResult(bool Success, string? GatewayOrderId, string? ClientSecret, string? PublicKey, string? Error);
public record GatewayVerifyResult(bool Success, string? Error);
public record GatewayRefundResult(bool Success, string? GatewayRefundId, string? Error);

public interface IPaymentGateway
{
    PaymentGatewayType GatewayType { get; }
    Task<GatewayOrderResult> CreateOrderAsync(string receiptId, decimal amount, string currency = "INR");
    Task<GatewayVerifyResult> VerifyPaymentAsync(string gatewayOrderId, string gatewayPaymentId, string? signature = null);
    Task<GatewayRefundResult> RefundAsync(string gatewayPaymentId, decimal amount);
}

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentGatewayType type);
}
