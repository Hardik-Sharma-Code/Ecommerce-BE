using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Get all users with optional role filter and pagination</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null)
    {
        var result = await _adminService.GetUsersAsync(page, pageSize, role);
        return Ok(result);
    }

    /// <summary>Enable or disable a user account</summary>
    [HttpPatch("users/{userId}/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableDisableUser(string userId, [FromBody] UserStatusUpdateDto dto)
    {
        var result = await _adminService.EnableDisableUserAsync(userId, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Permanently delete a user account</summary>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _adminService.DeleteUserAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get all customers with pagination</summary>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetCustomersAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get all vendors with pagination</summary>
    [HttpGet("vendors")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetVendorsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Approve or reject a vendor's KYC submission</summary>
    [HttpPatch("vendors/{vendorId}/kyc")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVendorKycStatus(string vendorId, [FromBody] UpdateVendorKycStatusDto dto)
    {
        var result = await _adminService.UpdateVendorKycStatusAsync(vendorId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
