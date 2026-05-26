using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    Task<Review?> GetByUserAndProductAsync(string userId, int productId);
    Task<PagedResult<Review>> GetByProductAsync(int productId, int page, int pageSize, bool approvedOnly = true);
    Task<PagedResult<Review>> GetAllAsync(int page, int pageSize, bool? isApproved = null);
    Task<PagedResult<Review>> GetByUserAsync(string userId, int page, int pageSize);
    Task<bool> HasPurchasedProductAsync(string userId, int productId);
    Task<(double Average, int Count)> GetRatingSummaryAsync(int productId);
    Task<Dictionary<int, int>> GetRatingBreakdownAsync(int productId);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
    Task SaveAsync();
}
