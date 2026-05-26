using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce_BE.Services.Implementations;

public class RazorpayGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly RazorpaySettings _settings;
    private readonly ILogger<RazorpayGateway> _logger;

    public PaymentGatewayType GatewayType => PaymentGatewayType.Razorpay;

    public RazorpayGateway(
        IHttpClientFactory factory,
        IOptions<RazorpaySettings> settings,
        ILogger<RazorpayGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = factory.CreateClient("Razorpay");

        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<GatewayOrderResult> CreateOrderAsync(string receiptId, decimal amount, string currency = "INR")
    {
        try
        {
            var body = new
            {
                amount = (int)(amount * 100), // paise
                currency,
                receipt = receiptId
            };

            var response = await _httpClient.PostAsJsonAsync("orders", body);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay order creation failed: {Content}", content);
                return new GatewayOrderResult(false, null, null, null, $"Gateway error: {content}");
            }

            var json = JsonDocument.Parse(content).RootElement;
            var orderId = json.GetProperty("id").GetString()!;

            return new GatewayOrderResult(true, orderId, null, _settings.KeyId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay CreateOrder exception");
            return new GatewayOrderResult(false, null, null, null, ex.Message);
        }
    }

    public Task<GatewayVerifyResult> VerifyPaymentAsync(
        string gatewayOrderId, string gatewayPaymentId, string? signature = null)
    {
        if (string.IsNullOrEmpty(signature))
            return Task.FromResult(new GatewayVerifyResult(false, "Signature is required for Razorpay verification."));

        var payload = $"{gatewayOrderId}|{gatewayPaymentId}";
        var keyBytes = Encoding.UTF8.GetBytes(_settings.KeySecret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var expected = BitConverter.ToString(hash).Replace("-", "").ToLower();

        return Task.FromResult(expected == signature.ToLower()
            ? new GatewayVerifyResult(true, null)
            : new GatewayVerifyResult(false, "Invalid payment signature."));
    }

    public async Task<GatewayRefundResult> RefundAsync(string gatewayPaymentId, decimal amount)
    {
        try
        {
            var body = new { amount = (int)(amount * 100) };
            var response = await _httpClient.PostAsJsonAsync($"payments/{gatewayPaymentId}/refund", body);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay refund failed: {Content}", content);
                return new GatewayRefundResult(false, null, $"Refund failed: {content}");
            }

            var json = JsonDocument.Parse(content).RootElement;
            var refundId = json.GetProperty("id").GetString()!;

            return new GatewayRefundResult(true, refundId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay Refund exception");
            return new GatewayRefundResult(false, null, ex.Message);
        }
    }
}
