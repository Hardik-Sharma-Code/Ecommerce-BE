using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Admin;

public class UpdateVendorKycStatusDto
{
    [Required]
    public KycStatus Status { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
