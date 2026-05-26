namespace Ecommerce_BE.Shared.Kernel.Models;

public enum ShipmentStatus
{
    Pending = 0,
    Shipped = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Failed = 5
}

public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string Carrier { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public ShipmentStatus Status { get; set; }

    public DateTime? ShippedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
}
