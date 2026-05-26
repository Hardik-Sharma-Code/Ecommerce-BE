using System.ComponentModel.DataAnnotations;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Shipping;

public class GetShippingRatesDto
{
    [Required]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = "IN";

    [Range(0, double.MaxValue)]
    public decimal OrderAmount { get; set; }
}
