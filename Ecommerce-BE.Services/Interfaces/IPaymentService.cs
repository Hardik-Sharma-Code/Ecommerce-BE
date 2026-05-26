using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Payment;

namespace Ecommerce_BE.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentInitiatedDto>> InitiateAsync(string userId, InitiatePaymentDto dto);
    Task<ApiResponse<PaymentDto>> VerifyRazorpayAsync(string userId, VerifyRazorpayDto dto);
    Task<ApiResponse<PaymentDto>> ConfirmStripeAsync(string userId, StripeConfirmDto dto);
    Task<ApiResponse<PaymentDto>> GetByOrderIdAsync(int orderId, string userId, string userRole);
    Task<ApiResponse> HandleRazorpayWebhookAsync(string body, string signature);
    Task<ApiResponse> HandleStripeWebhookAsync(string body, string signature);
}
