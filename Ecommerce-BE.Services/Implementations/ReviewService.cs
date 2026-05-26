using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Review;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;
    private readonly IProductRepository _products;

    public ReviewService(IReviewRepository reviews, IProductRepository products)
    {
        _reviews = reviews;
        _products = products;
    }

    public async Task<ApiResponse<ReviewDto>> GetByIdAsync(int id)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return ApiResponse<ReviewDto>.Fail("Review not found");
        return ApiResponse<ReviewDto>.Ok(MapToDto(review));
    }

    public async Task<ApiResponse<PagedResult<ReviewDto>>> GetByProductAsync(int productId, int page, int pageSize)
    {
        var result = await _reviews.GetByProductAsync(productId, page, pageSize, approvedOnly: true);
        return ApiResponse<PagedResult<ReviewDto>>.Ok(result.Map(MapToDto));
    }

    public async Task<ApiResponse<PagedResult<ReviewDto>>> GetMyReviewsAsync(string userId, int page, int pageSize)
    {
        var result = await _reviews.GetByUserAsync(userId, page, pageSize);
        return ApiResponse<PagedResult<ReviewDto>>.Ok(result.Map(MapToDto));
    }

    public async Task<ApiResponse<PagedResult<ReviewDto>>> GetAllAsync(int page, int pageSize, bool? isApproved)
    {
        var result = await _reviews.GetAllAsync(page, pageSize, isApproved);
        return ApiResponse<PagedResult<ReviewDto>>.Ok(result.Map(MapToDto));
    }

    public async Task<ApiResponse<ProductRatingSummaryDto>> GetRatingSummaryAsync(int productId)
    {
        var (average, count) = await _reviews.GetRatingSummaryAsync(productId);
        var breakdown = await _reviews.GetRatingBreakdownAsync(productId);

        return ApiResponse<ProductRatingSummaryDto>.Ok(new ProductRatingSummaryDto
        {
            ProductId = productId,
            AverageRating = Math.Round(average, 1),
            TotalReviews = count,
            RatingBreakdown = breakdown
        });
    }

    public async Task<ApiResponse<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto)
    {
        var product = await _products.GetByIdAsync(dto.ProductId);
        if (product == null) return ApiResponse<ReviewDto>.Fail("Product not found");

        var existing = await _reviews.GetByUserAndProductAsync(userId, dto.ProductId);
        if (existing != null) return ApiResponse<ReviewDto>.Fail("You have already reviewed this product");

        var isVerified = await _reviews.HasPurchasedProductAsync(userId, dto.ProductId);

        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Rating = dto.Rating,
            Title = dto.Title,
            Body = dto.Body,
            IsVerifiedPurchase = isVerified,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reviews.AddAsync(review);
        await _reviews.SaveAsync();

        var created = await _reviews.GetByIdAsync(review.Id);
        return ApiResponse<ReviewDto>.Ok(MapToDto(created!), "Review submitted and awaiting approval");
    }

    public async Task<ApiResponse<ReviewDto>> UpdateAsync(int id, string userId, UpdateReviewDto dto)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return ApiResponse<ReviewDto>.Fail("Review not found");
        if (review.UserId != userId) return ApiResponse<ReviewDto>.Fail("Access denied");

        review.Rating = dto.Rating;
        review.Title = dto.Title;
        review.Body = dto.Body;
        review.IsApproved = false; // re-submit for approval after edit
        review.UpdatedAt = DateTime.UtcNow;

        await _reviews.UpdateAsync(review);
        await _reviews.SaveAsync();

        return ApiResponse<ReviewDto>.Ok(MapToDto(review));
    }

    public async Task<ApiResponse> DeleteAsync(int id, string userId, bool isAdmin)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return ApiResponse.Fail("Review not found");
        if (!isAdmin && review.UserId != userId) return ApiResponse.Fail("Access denied");

        await _reviews.DeleteAsync(review);
        await _reviews.SaveAsync();

        return ApiResponse.Ok("Review deleted");
    }

    public async Task<ApiResponse<ReviewDto>> ApproveAsync(int id)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return ApiResponse<ReviewDto>.Fail("Review not found");

        review.IsApproved = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviews.UpdateAsync(review);
        await _reviews.SaveAsync();

        return ApiResponse<ReviewDto>.Ok(MapToDto(review));
    }

    private static ReviewDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        ProductName = r.Product?.Name ?? string.Empty,
        UserId = r.UserId,
        UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : string.Empty,
        Rating = r.Rating,
        Title = r.Title,
        Body = r.Body,
        IsVerifiedPurchase = r.IsVerifiedPurchase,
        IsApproved = r.IsApproved,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
