using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Order;

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
