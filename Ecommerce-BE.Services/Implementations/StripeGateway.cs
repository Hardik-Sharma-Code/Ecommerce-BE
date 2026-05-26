using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce_BE.Services.Implementations;

public class StripeGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeGateway> _logger;

    public PaymentGatewayType GatewayType => PaymentGatewayType.Stripe;

    public StripeGateway(
        IHttpClientFactory factory,
        IOptions<StripeSettings> settings,
        ILogger<StripeGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = factory.CreateClient("Stripe");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
    }

    public async Task<GatewayOrderResult> CreateOrderAsync(string receiptId, decimal amount, string currency = "INR")
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["amount"] = ((int)(amount * 100)).ToString(), // smallest currency unit
                ["currency"] = currency.ToLower(),
                ["metadata[receipt_id]"] = receiptId,
                ["automatic_payment_methods[enabled]"] = "true"
            });

            var response = await _httpClient.PostAsync("payment_intents", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Stripe PaymentIntent creation failed: {Body}", body);
                return new GatewayOrderResult(false, null, null, null, $"Stripe error: {body}");
            }

            var json = JsonDocument.Parse(body).RootElement;
            var intentId = json.GetProperty("id").GetString()!;
            var clientSecret = json.GetProperty("client_secret").GetString()!;

            return new GatewayOrderResult(true, intentId, clientSecret, _settings.PublishableKey, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe CreateOrder exception");
            return new GatewayOrderResult(false, null, null, null, ex.Message);
        }
    }

    public async Task<GatewayVerifyResult> VerifyPaymentAsync(
        string gatewayOrderId, string gatewayPaymentId, string? signature = null)
    {
        try
        {
            // For Stripe, gatewayOrderId = PaymentIntent ID; retrieve it to check status
            var response = await _httpClient.GetAsync($"payment_intents/{gatewayOrderId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new GatewayVerifyResult(false, "Could not retrieve payment intent from Stripe.");

            var json = JsonDocument.Parse(body).RootElement;
            var status = json.GetProperty("status").GetString();

            return status == "succeeded"
                ? new GatewayVerifyResult(true, null)
                : new GatewayVerifyResult(false, $"Payment not completed. Status: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe VerifyPayment exception");
            return new GatewayVerifyResult(false, ex.Message);
        }
    }

    public async Task<GatewayRefundResult> RefundAsync(string gatewayPaymentId, decimal amount)
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["payment_intent"] = gatewayPaymentId,
                ["amount"] = ((int)(amount * 100)).ToString()
            });

            var response = await _httpClient.PostAsync("refunds", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Stripe refund failed: {Body}", body);
                return new GatewayRefundResult(false, null, $"Stripe refund failed: {body}");
            }

            var json = JsonDocument.Parse(body).RootElement;
            var refundId = json.GetProperty("id").GetString()!;

            return new GatewayRefundResult(true, refundId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe Refund exception");
            return new GatewayRefundResult(false, null, ex.Message);
        }
    }

    // Stripe webhook signature verification
    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        try
        {
            // Stripe-Signature: t=timestamp,v1=signature
            var parts = signature.Split(',');
            var timestamp = parts.FirstOrDefault(p => p.StartsWith("t="))?.Substring(2);
            var v1 = parts.FirstOrDefault(p => p.StartsWith("v1="))?.Substring(3);

            if (timestamp is null || v1 is null) return false;

            var signedPayload = $"{timestamp}.{payload}";
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

            using var hmac = new HMACSHA256(secretBytes);
            var expected = BitConverter.ToString(hmac.ComputeHash(payloadBytes))
                .Replace("-", "").ToLower();

            return expected == v1.ToLower();
        }
        catch
        {
            return false;
        }
    }
}
