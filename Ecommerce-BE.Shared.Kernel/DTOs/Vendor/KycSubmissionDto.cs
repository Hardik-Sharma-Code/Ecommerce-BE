using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Vendor;

public class KycSubmissionDto
{
    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;
}
