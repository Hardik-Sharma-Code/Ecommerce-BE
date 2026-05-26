using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Shipping;

public class UpdateTrackingDto
{
    [Required, MaxLength(100)]
    public string Carrier { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TrackingNumber { get; set; }

    [MaxLength(500)]
    public string? TrackingUrl { get; set; }

    public DateTime? EstimatedDelivery { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
