using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Coupon;

namespace Ecommerce_BE.Services.Interfaces;

public interface ICouponService
{
    Task<ApiResponse<CouponValidationResultDto>> ValidateAsync(ApplyCouponDto dto);
    Task<ApiResponse<CouponDto>> GetByCodeAsync(string code);
    Task<ApiResponse<PagedResult<CouponDto>>> GetAllAsync(int page, int pageSize);
    Task<ApiResponse<CouponDto>> CreateAsync(CreateCouponDto dto);
    Task<ApiResponse<CouponDto>> UpdateAsync(int id, UpdateCouponDto dto);
    Task<ApiResponse> DeleteAsync(int id);
}
