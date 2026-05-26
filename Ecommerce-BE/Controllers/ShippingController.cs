using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    /// <summary>
    /// Get available shipping rates for a destination and order amount.
    /// Returns Standard (free above threshold), Express, and Overnight options.
    /// </summary>
    [HttpGet("rates")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShippingRateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRates([FromQuery] GetShippingRatesDto dto)
    {
        var result = await _shippingService.GetRatesAsync(dto);
        return Ok(result);
    }

    /// <summary>Get shipment / tracking info for an order</summary>
    [HttpGet("{orderId:int}/tracking")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTracking(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Customer;
        var result = await _shippingService.GetShipmentAsync(orderId, userId, userRole);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Add or update tracking information for an order [Admin]</summary>
    [HttpPost("{orderId:int}/tracking")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTracking(int orderId, [FromBody] UpdateTrackingDto dto)
    {
        var result = await _shippingService.UpdateTrackingAsync(orderId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update shipment status (InTransit → OutForDelivery → Delivered) [Admin]</summary>
    [HttpPatch("{orderId:int}/status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateShipmentStatusDto dto)
    {
        var result = await _shippingService.UpdateStatusAsync(orderId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
