namespace Ecommerce_BE.Shared.Kernel.Common;

public class ShippingSettings
{
    public decimal FreeShippingThreshold { get; set; } = 500m;
    public decimal StandardRate { get; set; } = 50m;
    public decimal ExpressRate { get; set; } = 150m;
    public decimal OvernightRate { get; set; } = 299m;
    public decimal TaxPercentage { get; set; } = 18m;
}
