using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Create a gateway order and get credentials for frontend SDK initialisation.
    /// Returns Razorpay order_id + key_id OR Stripe client_secret + publishable_key.
    /// </summary>
    [HttpPost("initiate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitiatedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _paymentService.InitiateAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Verify Razorpay payment signature after frontend checkout completes</summary>
    [HttpPost("verify/razorpay")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyRazorpay([FromBody] VerifyRazorpayDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _paymentService.VerifyRazorpayAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Confirm Stripe payment after PaymentIntent is completed on frontend</summary>
    [HttpPost("verify/stripe")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmStripe([FromBody] StripeConfirmDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _paymentService.ConfirmStripeAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get payment details for an order</summary>
    [HttpGet("order/{orderId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Customer;
        var result = await _paymentService.GetByOrderIdAsync(orderId, userId, userRole);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Razorpay webhook endpoint — verify X-Razorpay-Signature and update order status.
    /// Must be excluded from CSRF / authentication middleware.
    /// </summary>
    [HttpPost("webhook/razorpay")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RazorpayWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var signature = Request.Headers["X-Razorpay-Signature"].ToString();
        var result = await _paymentService.HandleRazorpayWebhookAsync(body, signature);
        return result.Success ? Ok() : BadRequest();
    }

    /// <summary>
    /// Stripe webhook endpoint — verify Stripe-Signature and update order status.
    /// Must be excluded from CSRF / authentication middleware.
    /// </summary>
    [HttpPost("webhook/stripe")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StripeWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        var result = await _paymentService.HandleStripeWebhookAsync(body, signature);
        return result.Success ? Ok() : BadRequest();
    }
}
