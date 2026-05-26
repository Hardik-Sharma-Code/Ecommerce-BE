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

    public ReportController(IReportService reportService) => _reportService = reportService;

    [HttpPost("sales")]
    public async Task<IActionResult> GetSalesReport([FromBody] ReportRequestDto request)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        if (!isAdmin)
        {
            // Vendor can only see their own data
            request.VendorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        var result = await _reportService.GetSalesReportAsync(request);
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport()
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        var vendorId = isAdmin ? null : User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _reportService.GetInventoryReportAsync(vendorId);
        return Ok(result);
    }

    [HttpPost("sales/export")]
    public async Task<IActionResult> ExportSalesReport([FromBody] ReportRequestDto request)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        if (!isAdmin)
            request.VendorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _reportService.ExportSalesReportCsvAsync(request);
        if (!result.Success) return BadRequest(result);

        var fileName = $"sales-report-{request.FromDate:yyyyMMdd}-{request.ToDate:yyyyMMdd}.csv";
        return File(result.Data!, "text/csv", fileName);
    }

    [HttpGet("inventory/export")]
    public async Task<IActionResult> ExportInventoryReport()
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        var vendorId = isAdmin ? null : User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _reportService.ExportInventoryReportCsvAsync(vendorId);
        if (!result.Success) return BadRequest(result);

        return File(result.Data!, "text/csv", "inventory-report.csv");
    }
}
