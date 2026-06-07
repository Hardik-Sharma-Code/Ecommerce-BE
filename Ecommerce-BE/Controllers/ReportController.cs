using System.Security.Claims;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_BE.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Vendor}")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IVendorService _vendorService;

    public ReportController(IReportService reportService, IVendorService vendorService)
    {
        _reportService = reportService;
        _vendorService = vendorService;
    }

    [HttpPost("sales")]
    public async Task<IActionResult> GetSalesReport([FromBody] ReportRequestDto request)
    {
        if (!User.IsInRole(Roles.Admin))
        {
            var vendorProfileId = await ResolveVendorProfileIdAsync();
            if (vendorProfileId is null) return BadRequest("Vendor profile not found.");
            request.VendorId = vendorProfileId;
        }

        var result = await _reportService.GetSalesReportAsync(request);
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport()
    {
        int? vendorId = null;
        if (!User.IsInRole(Roles.Admin))
        {
            vendorId = await ResolveVendorProfileIdAsync();
            if (vendorId is null) return BadRequest("Vendor profile not found.");
        }

        var result = await _reportService.GetInventoryReportAsync(vendorId);
        return Ok(result);
    }

    [HttpPost("sales/export")]
    public async Task<IActionResult> ExportSalesReport([FromBody] ReportRequestDto request)
    {
        if (!User.IsInRole(Roles.Admin))
        {
            var vendorProfileId = await ResolveVendorProfileIdAsync();
            if (vendorProfileId is null) return BadRequest("Vendor profile not found.");
            request.VendorId = vendorProfileId;
        }

        var result = await _reportService.ExportSalesReportCsvAsync(request);
        if (!result.Success) return BadRequest(result);

        var fileName = $"sales-report-{request.FromDate:yyyyMMdd}-{request.ToDate:yyyyMMdd}.csv";
        return File(result.Data!, "text/csv", fileName);
    }

    [HttpGet("inventory/export")]
    public async Task<IActionResult> ExportInventoryReport()
    {
        int? vendorId = null;
        if (!User.IsInRole(Roles.Admin))
        {
            vendorId = await ResolveVendorProfileIdAsync();
            if (vendorId is null) return BadRequest("Vendor profile not found.");
        }

        var result = await _reportService.ExportInventoryReportCsvAsync(vendorId);
        if (!result.Success) return BadRequest(result);

        return File(result.Data!, "text/csv", "inventory-report.csv");
    }

    private async Task<int?> ResolveVendorProfileIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _vendorService.GetProfileAsync(userId);
        return profile.Success ? profile.Data!.Id : null;
    }
}
