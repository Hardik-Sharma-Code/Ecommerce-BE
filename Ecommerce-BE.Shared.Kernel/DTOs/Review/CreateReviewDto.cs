using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Review;

public class CreateReviewDto
{
    [Required]
    public int ProductId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;
}
