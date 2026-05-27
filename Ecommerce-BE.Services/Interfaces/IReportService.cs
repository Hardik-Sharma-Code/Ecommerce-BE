using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Report;

namespace Ecommerce_BE.Services.Interfaces;

public interface IReportService
{
    Task<ApiResponse<SalesReportDto>> GetSalesReportAsync(ReportRequestDto request);
    Task<ApiResponse<InventoryReportDto>> GetInventoryReportAsync(string? vendorId = null);
    Task<ApiResponse<byte[]>> ExportSalesReportCsvAsync(ReportRequestDto request);
    Task<ApiResponse<byte[]>> ExportInventoryReportCsvAsync(string? vendorId = null);
}
