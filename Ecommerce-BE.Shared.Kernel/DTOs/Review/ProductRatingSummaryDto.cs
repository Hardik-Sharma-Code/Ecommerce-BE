namespace Ecommerce_BE.Shared.Kernel.DTOs.Review;

public class ProductRatingSummaryDto
{
    public int ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingBreakdown { get; set; } = new(); // star -> count
}
