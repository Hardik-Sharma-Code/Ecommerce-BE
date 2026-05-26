using Ecommerce_BE.Shared.Kernel.DTOs.Payment;
using Ecommerce_BE.Shared.Kernel.DTOs.Refund;
using Ecommerce_BE.Shared.Kernel.DTOs.Shipping;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();

    public string ShippingFirstName { get; set; } = string.Empty;
    public string ShippingLastName { get; set; } = string.Empty;
    public string? ShippingPhone { get; set; }
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? CouponCode { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
    public DateTime PlacedAt { get; set; }

    public IEnumerable<OrderItemDto> Items { get; set; } = Enumerable.Empty<OrderItemDto>();
    public PaymentDto? Payment { get; set; }
    public ShipmentDto? Shipment { get; set; }
    public IEnumerable<RefundDto> Refunds { get; set; } = Enumerable.Empty<RefundDto>();
}
