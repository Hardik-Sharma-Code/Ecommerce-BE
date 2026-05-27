using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Order;

public class CancelOrderDto
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}
