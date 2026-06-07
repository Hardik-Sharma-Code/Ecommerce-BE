using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IVendorService _vendorService;

    public DashboardController(IDashboardService dashboardService, IVendorService vendorService)
    {
        _dashboardService = dashboardService;
        _vendorService = vendorService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var result = await _dashboardService.GetAdminDashboardAsync();
        return Ok(result);
    }

    [HttpGet("vendor")]
    [Authorize(Roles = Roles.Vendor)]
    public async Task<IActionResult> GetVendorDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profileResult = await _vendorService.GetProfileAsync(userId);
        if (!profileResult.Success)
            return BadRequest(profileResult);

        var result = await _dashboardService.GetVendorDashboardAsync(profileResult.Data!.Id);
        return Ok(result);
    }
}
