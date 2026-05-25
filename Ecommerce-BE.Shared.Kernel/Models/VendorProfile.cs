namespace Ecommerce_BE.Shared.Kernel.Models;

public class VendorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessDescription { get; set; }
    public string? BusinessAddress { get; set; }
    public string? BusinessPhone { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public string? KycDocumentType { get; set; }
    public string? KycDocumentNumber { get; set; }
    public string? KycRejectionReason { get; set; }
    public DateTime? KycSubmittedAt { get; set; }
    public DateTime? KycReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}

public enum KycStatus
{
    Pending,
    Submitted,
    Approved,
    Rejected
}
