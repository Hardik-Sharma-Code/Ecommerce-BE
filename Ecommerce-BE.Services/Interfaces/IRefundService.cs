using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Refund;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Interfaces;

public interface IRefundService
{
    Task<ApiResponse<RefundDto>> RequestRefundAsync(string userId, RequestRefundDto dto);
    Task<ApiResponse<IEnumerable<RefundDto>>> GetMyRefundsAsync(string userId);
    Task<ApiResponse<PagedResult<RefundDto>>> GetAllRefundsAsync(int page, int pageSize, RefundStatus? status);
    Task<ApiResponse<RefundDto>> GetByIdAsync(int id, string userId, string userRole);
    Task<ApiResponse<RefundDto>> ProcessRefundAsync(int id, ProcessRefundDto dto);
}
