using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Refund;

public class ProcessRefundDto
{
    [Required]
    public RefundStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
