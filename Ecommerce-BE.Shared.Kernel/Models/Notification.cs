namespace Ecommerce_BE.Shared.Kernel.Models;

public enum NotificationType
{
    OrderPlaced = 0,
    OrderConfirmed = 1,
    OrderShipped = 2,
    OrderDelivered = 3,
    OrderCancelled = 4,
    RefundRequested = 5,
    RefundProcessed = 6,
    ReviewApproved = 7,
    LowStock = 8,
    Promotion = 9,
    System = 10
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
