using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Dashboard;

namespace Ecommerce_BE.Services.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync();
    Task<ApiResponse<VendorDashboardDto>> GetVendorDashboardAsync(string vendorId);
}
