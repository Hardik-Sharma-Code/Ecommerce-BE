using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Payment;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce_BE.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly RazorpaySettings _razorpaySettings;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IPaymentGatewayFactory gatewayFactory,
        IOptions<RazorpaySettings> razorpaySettings,
        IOptions<StripeSettings> stripeSettings,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _gatewayFactory = gatewayFactory;
        _razorpaySettings = razorpaySettings.Value;
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<PaymentInitiatedDto>> InitiateAsync(string userId, InitiatePaymentDto dto)
    {
        var order = await _orderRepository.GetByIdForUserAsync(dto.OrderId, userId);
        if (order is null) return ApiResponse<PaymentInitiatedDto>.Fail("Order not found.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            return ApiResponse<PaymentInitiatedDto>.Fail("Order is already paid.");

        if (order.Status == OrderStatus.Cancelled)
            return ApiResponse<PaymentInitiatedDto>.Fail("Cannot pay for a cancelled order.");

        var gateway = _gatewayFactory.GetGateway(dto.Gateway);
        var result = await gateway.CreateOrderAsync(order.OrderNumber, order.TotalAmount, dto.Currency);

        if (!result.Success)
            return ApiResponse<PaymentInitiatedDto>.Fail(result.Error ?? "Payment gateway error.");

        // Create or update the pending payment record
        var payment = await _paymentRepository.GetByOrderIdAsync(dto.OrderId);
        if (payment is null)
        {
            payment = new Payment
            {
                OrderId = dto.OrderId,
                Gateway = dto.Gateway,
                GatewayOrderId = result.GatewayOrderId,
                Amount = order.TotalAmount,
                Currency = dto.Currency,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.CreateAsync(payment);
        }
        else
        {
            payment.Gateway = dto.Gateway;
            payment.GatewayOrderId = result.GatewayOrderId;
            payment.Status = PaymentStatus.Pending;
            await _paymentRepository.UpdateAsync(payment);
        }

        return ApiResponse<PaymentInitiatedDto>.Ok(new PaymentInitiatedDto
        {
            OrderId = dto.OrderId,
            GatewayOrderId = result.GatewayOrderId!,
            ClientSecret = result.ClientSecret,
            PublicKey = result.PublicKey!,
            Amount = order.TotalAmount,
            Currency = dto.Currency,
            Gateway = dto.Gateway
        });
    }

    public async Task<ApiResponse<PaymentDto>> VerifyRazorpayAsync(string userId, VerifyRazorpayDto dto)
    {
        var order = await _orderRepository.GetByIdForUserAsync(dto.OrderId, userId);
        if (order is null) return ApiResponse<PaymentDto>.Fail("Order not found.");

        var payment = await _paymentRepository.GetByOrderIdAsync(dto.OrderId);
        if (payment is null) return ApiResponse<PaymentDto>.Fail("Payment record not found. Please initiate payment first.");

        var gateway = _gatewayFactory.GetGateway(PaymentGatewayType.Razorpay);
        var verifyResult = await gateway.VerifyPaymentAsync(
            dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature);

        if (!verifyResult.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = verifyResult.Error;
            await _paymentRepository.UpdateAsync(payment);
            return ApiResponse<PaymentDto>.Fail(verifyResult.Error ?? "Payment verification failed.");
        }

        payment.GatewayPaymentId = dto.RazorpayPaymentId;
        payment.GatewaySignature = dto.RazorpaySignature;
        payment.Status = PaymentStatus.Paid;
        await _paymentRepository.UpdateAsync(payment);

        order.PaymentStatus = PaymentStatus.Paid;
        order.Status = OrderStatus.Confirmed;
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Razorpay payment verified for order {OrderNumber}", order.OrderNumber);
        return ApiResponse<PaymentDto>.Ok(MapToDto(payment), "Payment verified successfully.");
    }

    public async Task<ApiResponse<PaymentDto>> ConfirmStripeAsync(string userId, StripeConfirmDto dto)
    {
        var order = await _orderRepository.GetByIdForUserAsync(dto.OrderId, userId);
        if (order is null) return ApiResponse<PaymentDto>.Fail("Order not found.");

        var payment = await _paymentRepository.GetByOrderIdAsync(dto.OrderId);
        if (payment is null) return ApiResponse<PaymentDto>.Fail("Payment record not found. Please initiate payment first.");

        var gateway = _gatewayFactory.GetGateway(PaymentGatewayType.Stripe);
        var verifyResult = await gateway.VerifyPaymentAsync(dto.PaymentIntentId, dto.PaymentIntentId);

        if (!verifyResult.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = verifyResult.Error;
            await _paymentRepository.UpdateAsync(payment);
            return ApiResponse<PaymentDto>.Fail(verifyResult.Error ?? "Stripe payment not completed.");
        }

        payment.GatewayPaymentId = dto.PaymentIntentId;
        payment.Status = PaymentStatus.Paid;
        await _paymentRepository.UpdateAsync(payment);

        order.PaymentStatus = PaymentStatus.Paid;
        order.Status = OrderStatus.Confirmed;
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Stripe payment confirmed for order {OrderNumber}", order.OrderNumber);
        return ApiResponse<PaymentDto>.Ok(MapToDto(payment), "Payment confirmed successfully.");
    }

    public async Task<ApiResponse<PaymentDto>> GetByOrderIdAsync(int orderId, string userId, string userRole)
    {
        var order = userRole == Roles.Admin
            ? await _orderRepository.GetByIdAsync(orderId)
            : await _orderRepository.GetByIdForUserAsync(orderId, userId);

        if (order is null) return ApiResponse<PaymentDto>.Fail("Order not found.");

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment is null) return ApiResponse<PaymentDto>.Fail("No payment record for this order.");

        return ApiResponse<PaymentDto>.Ok(MapToDto(payment));
    }

    public async Task<ApiResponse> HandleRazorpayWebhookAsync(string body, string signature)
    {
        // Verify webhook signature: HMAC-SHA256(body, webhook_secret)
        var keyBytes = Encoding.UTF8.GetBytes(_razorpaySettings.WebhookSecret);
        var payloadBytes = Encoding.UTF8.GetBytes(body);
        using var hmac = new HMACSHA256(keyBytes);
        var expected = BitConverter.ToString(hmac.ComputeHash(payloadBytes)).Replace("-", "").ToLower();

        if (expected != signature?.ToLower())
            return ApiResponse.Fail("Invalid webhook signature.");

        try
        {
            var json = JsonDocument.Parse(body).RootElement;
            var eventType = json.GetProperty("event").GetString();

            if (eventType == "payment.captured")
            {
                var gatewayPaymentId = json.GetProperty("payload")
                    .GetProperty("payment").GetProperty("entity")
                    .GetProperty("id").GetString();

                var gatewayOrderId = json.GetProperty("payload")
                    .GetProperty("payment").GetProperty("entity")
                    .GetProperty("order_id").GetString();

                if (gatewayOrderId is not null)
                {
                    var payment = await _paymentRepository.GetByGatewayOrderIdAsync(gatewayOrderId);
                    if (payment is not null && payment.Status != PaymentStatus.Paid)
                    {
                        payment.GatewayPaymentId = gatewayPaymentId;
                        payment.Status = PaymentStatus.Paid;
                        await _paymentRepository.UpdateAsync(payment);

                        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
                        if (order is not null)
                        {
                            order.PaymentStatus = PaymentStatus.Paid;
                            order.Status = OrderStatus.Confirmed;
                            await _orderRepository.UpdateAsync(order);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay webhook processing error");
        }

        return ApiResponse.Ok("Webhook processed.");
    }

    public async Task<ApiResponse> HandleStripeWebhookAsync(string body, string signature)
    {
        // Stripe gateway exposes VerifyWebhookSignature
        var stripeGateway = _gatewayFactory.GetGateway(PaymentGatewayType.Stripe) as StripeGateway;
        if (stripeGateway is null || !stripeGateway.VerifyWebhookSignature(body, signature, _stripeSettings.WebhookSecret))
            return ApiResponse.Fail("Invalid webhook signature.");

        try
        {
            var json = JsonDocument.Parse(body).RootElement;
            var eventType = json.GetProperty("type").GetString();

            if (eventType == "payment_intent.succeeded")
            {
                var intentId = json.GetProperty("data").GetProperty("object").GetProperty("id").GetString();

                if (intentId is not null)
                {
                    var payment = await _paymentRepository.GetByGatewayOrderIdAsync(intentId);
                    if (payment is not null && payment.Status != PaymentStatus.Paid)
                    {
                        payment.GatewayPaymentId = intentId;
                        payment.Status = PaymentStatus.Paid;
                        await _paymentRepository.UpdateAsync(payment);

                        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
                        if (order is not null)
                        {
                            order.PaymentStatus = PaymentStatus.Paid;
                            order.Status = OrderStatus.Confirmed;
                            await _orderRepository.UpdateAsync(order);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook processing error");
        }

        return ApiResponse.Ok("Webhook processed.");
    }

    private static PaymentDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        Gateway = p.Gateway,
        GatewayOrderId = p.GatewayOrderId,
        GatewayPaymentId = p.GatewayPaymentId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        FailureReason = p.FailureReason,
        CreatedAt = p.CreatedAt
    };
}
