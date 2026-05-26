using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Coupon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    /// <summary>Validate a coupon code and calculate the discount [Authenticated]</summary>
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CouponValidationResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ApplyCouponDto dto)
    {
        var result = await _couponService.ValidateAsync(dto);
        return Ok(result);
    }

    /// <summary>Get a coupon by code [Admin]</summary>
    [HttpGet("{code}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _couponService.GetByCodeAsync(code);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get all coupons (paginated) [Admin]</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CouponDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _couponService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Create a new coupon [Admin]</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
    {
        var result = await _couponService.CreateAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetByCode), new { code = result.Data!.Code }, result)
            : BadRequest(result);
    }

    /// <summary>Update an existing coupon [Admin]</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponDto dto)
    {
        var result = await _couponService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a coupon [Admin]</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _couponService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
