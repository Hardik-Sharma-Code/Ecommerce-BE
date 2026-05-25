using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Vendor;

public class UpdateVendorProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? BusinessDescription { get; set; }

    [Phone]
    public string? BusinessPhone { get; set; }

    [MaxLength(500)]
    public string? BusinessAddress { get; set; }
}
