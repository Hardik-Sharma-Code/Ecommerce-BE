using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Shipping;

public class ShipmentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public ShipmentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? ShippedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
}
