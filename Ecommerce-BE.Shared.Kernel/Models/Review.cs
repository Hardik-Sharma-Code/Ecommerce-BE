namespace Ecommerce_BE.Shared.Kernel.Models;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int Rating { get; set; }          // 1–5
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
