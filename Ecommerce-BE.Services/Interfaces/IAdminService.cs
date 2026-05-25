using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Admin;

namespace Ecommerce_BE.Services.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<PagedResult<UserListDto>>> GetUsersAsync(int page, int pageSize, string? role = null);
    Task<ApiResponse> EnableDisableUserAsync(string userId, UserStatusUpdateDto dto);
    Task<ApiResponse> DeleteUserAsync(string userId);
    Task<ApiResponse<PagedResult<UserListDto>>> GetCustomersAsync(int page, int pageSize);
    Task<ApiResponse<PagedResult<UserListDto>>> GetVendorsAsync(int page, int pageSize);
    Task<ApiResponse> UpdateVendorKycStatusAsync(string vendorId, UpdateVendorKycStatusDto dto);
}
