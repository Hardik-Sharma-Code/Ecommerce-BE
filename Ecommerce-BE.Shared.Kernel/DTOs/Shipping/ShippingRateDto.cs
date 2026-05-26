namespace Ecommerce_BE.Shared.Kernel.DTOs.Shipping;

public class ShippingRateDto
{
    public string Method { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int MinDays { get; set; }
    public int MaxDays { get; set; }
    public string Description { get; set; } = string.Empty;
}
