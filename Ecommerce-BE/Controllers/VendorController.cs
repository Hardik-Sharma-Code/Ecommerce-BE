using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Vendor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    /// <summary>Register a new vendor account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<VendorProfileDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] VendorRegistrationDto dto)
    {
        var result = await _vendorService.RegisterAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetProfile), result) : BadRequest(result);
    }

    /// <summary>Get the authenticated vendor's profile</summary>
    [HttpGet("profile")]
    [Authorize(Roles = Roles.Vendor)]
    [ProducesResponseType(typeof(ApiResponse<VendorProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _vendorService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update the authenticated vendor's profile</summary>
    [HttpPut("profile")]
    [Authorize(Roles = Roles.Vendor)]
    [ProducesResponseType(typeof(ApiResponse<VendorProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateVendorProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _vendorService.UpdateProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Submit KYC documents for verification</summary>
    [HttpPost("kyc/submit")]
    [Authorize(Roles = Roles.Vendor)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitKyc([FromBody] KycSubmissionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _vendorService.SubmitKycAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get current KYC verification status</summary>
    [HttpGet("kyc/status")]
    [Authorize(Roles = Roles.Vendor)]
    [ProducesResponseType(typeof(ApiResponse<KycStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKycStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _vendorService.GetKycStatusAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
