using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Shipping;

public class UpdateShipmentStatusDto
{
    [Required]
    public ShipmentStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
