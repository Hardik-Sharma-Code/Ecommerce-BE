using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Order;
using Ecommerce_BE.Shared.Kernel.DTOs.Payment;
using Ecommerce_BE.Shared.Kernel.DTOs.Refund;
using Ecommerce_BE.Shared.Kernel.DTOs.Shipping;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce_BE.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IProductRepository _productRepository;
    private readonly ShippingSettings _shippingSettings;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IAddressRepository addressRepository,
        ICouponRepository couponRepository,
        IProductRepository productRepository,
        IOptions<ShippingSettings> shippingSettings,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _addressRepository = addressRepository;
        _couponRepository = couponRepository;
        _productRepository = productRepository;
        _shippingSettings = shippingSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderDto>> GetOrderAsync(int orderId, string userId, string userRole)
    {
        var order = userRole == Roles.Admin
            ? await _orderRepository.GetByIdAsync(orderId)
            : await _orderRepository.GetByIdForUserAsync(orderId, userId);

        if (order is null) return ApiResponse<OrderDto>.Fail("Order not found.");
        return ApiResponse<OrderDto>.Ok(MapToDto(order));
    }

    public async Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetMyOrdersAsync(
        string userId, int page, int pageSize)
    {
        var (orders, total) = await _orderRepository.GetByUserIdAsync(userId, page, pageSize);
        return ApiResponse<PagedResult<OrderSummaryDto>>.Ok(new PagedResult<OrderSummaryDto>
        {
            Items = orders.Select(MapToSummaryDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetAllOrdersAsync(
        int page, int pageSize, OrderStatus? status)
    {
        var (orders, total) = await _orderRepository.GetAllAsync(page, pageSize, status);
        return ApiResponse<PagedResult<OrderSummaryDto>>.Ok(new PagedResult<OrderSummaryDto>
        {
            Items = orders.Select(MapToSummaryDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<OrderDto>> PlaceOrderAsync(string userId, PlaceOrderDto dto)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null || !cart.Items.Any())
            return ApiResponse<OrderDto>.Fail("Cart is empty.");

        var address = await _addressRepository.GetByIdAsync(dto.AddressId);
        if (address is null || address.UserId != userId)
            return ApiResponse<OrderDto>.Fail("Shipping address not found.");

        if (!new[] { "Standard", "Express", "Overnight" }.Contains(dto.ShippingMethod))
            return ApiResponse<OrderDto>.Fail("Invalid shipping method. Use: Standard, Express, or Overnight.");

        // Validate stock for all items up-front
        var products = new Dictionary<int, Product>();
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product is null || !product.IsActive)
                return ApiResponse<OrderDto>.Fail($"Product '{cartItem.Product?.Name ?? cartItem.ProductId.ToString()}' is no longer available.");
            if (product.StockQuantity < cartItem.Quantity)
                return ApiResponse<OrderDto>.Fail($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}.");
            products[cartItem.ProductId] = product;
        }

        var subtotal = cart.Items.Sum(i => products[i.ProductId].Price * i.Quantity);

        // Validate and apply coupon
        decimal discountAmount = 0;
        string? appliedCouponCode = null;
        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(dto.CouponCode);
            var couponError = ValidateCoupon(coupon, subtotal);
            if (couponError is not null)
                return ApiResponse<OrderDto>.Fail(couponError);

            discountAmount = CalculateCouponDiscount(coupon!, subtotal);
            appliedCouponCode = coupon!.Code;
        }

        var discountedSubtotal = subtotal - discountAmount;
        var shippingAmount = GetShippingRate(dto.ShippingMethod, discountedSubtotal);
        var taxAmount = Math.Round(discountedSubtotal * (_shippingSettings.TaxPercentage / 100m), 2);
        var totalAmount = discountedSubtotal + taxAmount + shippingAmount;

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            ShippingFirstName = address.FirstName,
            ShippingLastName = address.LastName,
            ShippingPhone = address.PhoneNumber,
            ShippingAddressLine1 = address.AddressLine1,
            ShippingAddressLine2 = address.AddressLine2,
            ShippingCity = address.City,
            ShippingState = address.State,
            ShippingPostalCode = address.PostalCode,
            ShippingCountry = address.Country,
            Subtotal = subtotal,
            ShippingAmount = shippingAmount,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            CouponCode = appliedCouponCode,
            Notes = dto.Notes,
            PlacedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = cart.Items.Select(i =>
            {
                var p = products[i.ProductId];
                return new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = p.Name,
                    SKU = p.SKU,
                    ImageUrl = p.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                             ?? p.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = p.Price,
                    Quantity = i.Quantity,
                    Subtotal = p.Price * i.Quantity
                };
            }).ToList()
        };

        await _orderRepository.CreateAsync(order);

        // Decrement stock and clear cart after successful order creation
        foreach (var cartItem in cart.Items)
            await _productRepository.DecrementStockAsync(cartItem.ProductId, cartItem.Quantity);

        if (appliedCouponCode is not null)
        {
            var coupon = await _couponRepository.GetByCodeAsync(appliedCouponCode);
            if (coupon is not null)
                await _couponRepository.IncrementUsageAsync(coupon.Id);
        }

        await _cartRepository.ClearItemsAsync(cart.Id);
        await _cartRepository.SaveAsync();

        _logger.LogInformation("Order {OrderNumber} placed by user {UserId}, total={Total}",
            order.OrderNumber, userId, totalAmount);

        return ApiResponse<OrderDto>.Ok(MapToDto(order), "Order placed successfully.");
    }

    public async Task<ApiResponse> CancelOrderAsync(int orderId, string userId, string userRole, string? reason)
    {
        var order = userRole == Roles.Admin
            ? await _orderRepository.GetByIdAsync(orderId)
            : await _orderRepository.GetByIdForUserAsync(orderId, userId);

        if (order is null) return ApiResponse.Fail("Order not found.");

        var cancellableStatuses = userRole == Roles.Admin
            ? new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Processing }
            : new[] { OrderStatus.Pending, OrderStatus.Confirmed };

        if (!cancellableStatuses.Contains(order.Status))
            return ApiResponse.Fail($"Order in '{order.Status}' status cannot be cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason;
        await _orderRepository.UpdateAsync(order);

        return ApiResponse.Ok("Order cancelled.");
    }

    public async Task<ApiResponse<OrderDto>> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null) return ApiResponse<OrderDto>.Fail("Order not found.");

        order.Status = dto.Status;
        if (dto.Notes is not null) order.Notes = dto.Notes;

        await _orderRepository.UpdateAsync(order);
        return ApiResponse<OrderDto>.Ok(MapToDto(order), "Order status updated.");
    }

    private decimal GetShippingRate(string method, decimal discountedSubtotal) => method switch
    {
        "Express" => _shippingSettings.ExpressRate,
        "Overnight" => _shippingSettings.OvernightRate,
        _ => discountedSubtotal >= _shippingSettings.FreeShippingThreshold ? 0 : _shippingSettings.StandardRate
    };

    private static decimal CalculateCouponDiscount(Coupon coupon, decimal subtotal)
    {
        var discount = coupon.DiscountType == DiscountType.Percentage
            ? subtotal * (coupon.DiscountValue / 100m)
            : coupon.DiscountValue;

        if (coupon.MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

        return Math.Round(Math.Min(discount, subtotal), 2);
    }

    private static string? ValidateCoupon(Coupon? coupon, decimal subtotal)
    {
        if (coupon is null) return "Coupon code not found.";
        if (!coupon.IsActive) return "This coupon is inactive.";
        var now = DateTime.UtcNow;
        if (now < coupon.ValidFrom) return "This coupon is not yet valid.";
        if (now > coupon.ValidTo) return "This coupon has expired.";
        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            return "This coupon has reached its usage limit.";
        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
            return $"Minimum order amount of {coupon.MinOrderAmount:C} required.";
        return null;
    }

    private static string GenerateOrderNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"ORD-{date}-{suffix}";
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Status = o.Status,
        PaymentStatus = o.PaymentStatus,
        ShippingFirstName = o.ShippingFirstName,
        ShippingLastName = o.ShippingLastName,
        ShippingPhone = o.ShippingPhone,
        ShippingAddressLine1 = o.ShippingAddressLine1,
        ShippingAddressLine2 = o.ShippingAddressLine2,
        ShippingCity = o.ShippingCity,
        ShippingState = o.ShippingState,
        ShippingPostalCode = o.ShippingPostalCode,
        ShippingCountry = o.ShippingCountry,
        Subtotal = o.Subtotal,
        ShippingAmount = o.ShippingAmount,
        DiscountAmount = o.DiscountAmount,
        TaxAmount = o.TaxAmount,
        TotalAmount = o.TotalAmount,
        CouponCode = o.CouponCode,
        CancellationReason = o.CancellationReason,
        Notes = o.Notes,
        PlacedAt = o.PlacedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            SKU = i.SKU,
            ImageUrl = i.ImageUrl,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            Subtotal = i.Subtotal
        }),
        Payment = o.Payment is null ? null : new PaymentDto
        {
            Id = o.Payment.Id,
            OrderId = o.Payment.OrderId,
            Gateway = o.Payment.Gateway,
            GatewayOrderId = o.Payment.GatewayOrderId,
            GatewayPaymentId = o.Payment.GatewayPaymentId,
            Amount = o.Payment.Amount,
            Currency = o.Payment.Currency,
            Status = o.Payment.Status,
            FailureReason = o.Payment.FailureReason,
            CreatedAt = o.Payment.CreatedAt
        },
        Shipment = o.Shipment is null ? null : new ShipmentDto
        {
            Id = o.Shipment.Id,
            OrderId = o.Shipment.OrderId,
            Carrier = o.Shipment.Carrier,
            TrackingNumber = o.Shipment.TrackingNumber,
            TrackingUrl = o.Shipment.TrackingUrl,
            Status = o.Shipment.Status,
            ShippedAt = o.Shipment.ShippedAt,
            EstimatedDelivery = o.Shipment.EstimatedDelivery,
            DeliveredAt = o.Shipment.DeliveredAt,
            Notes = o.Shipment.Notes
        },
        Refunds = o.Refunds.Select(r => new RefundDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            OrderNumber = o.OrderNumber,
            Amount = r.Amount,
            Reason = r.Reason,
            Status = r.Status,
            GatewayRefundId = r.GatewayRefundId,
            Notes = r.Notes,
            RequestedAt = r.RequestedAt,
            ProcessedAt = r.ProcessedAt
        })
    };

    private static OrderSummaryDto MapToSummaryDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Status = o.Status,
        PaymentStatus = o.PaymentStatus,
        TotalAmount = o.TotalAmount,
        ItemCount = o.Items.Sum(i => i.Quantity),
        PlacedAt = o.PlacedAt
    };
}
