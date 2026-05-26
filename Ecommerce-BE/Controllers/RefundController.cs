using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Refund;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class RefundController : ControllerBase
{
    private readonly IRefundService _refundService;

    public RefundController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    /// <summary>Request a refund for a delivered order [Customer]</summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(ApiResponse<RefundDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Request([FromBody] RequestRefundDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _refundService.RequestRefundAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get the current user's refund requests</summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RefundDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _refundService.GetMyRefundsAsync(userId);
        return Ok(result);
    }

    /// <summary>Get all refund requests (paginated, filterable by status) [Admin]</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RefundDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] RefundStatus? status = null)
    {
        var result = await _refundService.GetAllRefundsAsync(page, pageSize, status);
        return Ok(result);
    }

    /// <summary>Get a refund by ID (customer: own; admin: any)</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RefundDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.Customer;
        var result = await _refundService.GetByIdAsync(id, userId, userRole);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Process a refund — update status and optionally trigger gateway refund.
    /// Setting status to Completed calls the payment gateway. [Admin]
    /// </summary>
    [HttpPatch("{id:int}/process")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<RefundDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Process(int id, [FromBody] ProcessRefundDto dto)
    {
        var result = await _refundService.ProcessRefundAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
