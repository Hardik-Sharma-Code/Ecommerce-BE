using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Review;

namespace Ecommerce_BE.Services.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewDto>> GetByIdAsync(int id);
    Task<ApiResponse<PagedResult<ReviewDto>>> GetByProductAsync(int productId, int page, int pageSize);
    Task<ApiResponse<PagedResult<ReviewDto>>> GetMyReviewsAsync(string userId, int page, int pageSize);
    Task<ApiResponse<PagedResult<ReviewDto>>> GetAllAsync(int page, int pageSize, bool? isApproved);
    Task<ApiResponse<ProductRatingSummaryDto>> GetRatingSummaryAsync(int productId);
    Task<ApiResponse<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto);
    Task<ApiResponse<ReviewDto>> UpdateAsync(int id, string userId, UpdateReviewDto dto);
    Task<ApiResponse> DeleteAsync(int id, string userId, bool isAdmin);
    Task<ApiResponse<ReviewDto>> ApproveAsync(int id);
}
