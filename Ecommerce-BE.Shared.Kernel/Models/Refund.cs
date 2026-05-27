namespace Ecommerce_BE.Shared.Kernel.Models;

public enum RefundStatus { Requested = 0, Processing = 1, Completed = 2, Failed = 3 }

public class Refund
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RefundStatus Status { get; set; }

    public string? GatewayRefundId { get; set; }
    public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
