using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Shipping;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce_BE.Services.Implementations;

public class ShippingService : IShippingService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ShippingSettings _settings;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(
        IShipmentRepository shipmentRepository,
        IOrderRepository orderRepository,
        IOptions<ShippingSettings> settings,
        ILogger<ShippingService> logger)
    {
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<ApiResponse<IEnumerable<ShippingRateDto>>> GetRatesAsync(GetShippingRatesDto dto)
    {
        var rates = new List<ShippingRateDto>
        {
            new()
            {
                Method = "Standard",
                Carrier = "BlueDart",
                ServiceName = "Standard Delivery",
                Rate = dto.OrderAmount >= _settings.FreeShippingThreshold ? 0 : _settings.StandardRate,
                MinDays = 3,
                MaxDays = 7,
                Description = dto.OrderAmount >= _settings.FreeShippingThreshold
                    ? "Free Standard Delivery"
                    : $"Standard Delivery ({_settings.StandardRate:C})"
            },
            new()
            {
                Method = "Express",
                Carrier = "FedEx",
                ServiceName = "Express Delivery",
                Rate = _settings.ExpressRate,
                MinDays = 1,
                MaxDays = 2,
                Description = $"Express Delivery (1-2 days) — {_settings.ExpressRate:C}"
            },
            new()
            {
                Method = "Overnight",
                Carrier = "DHL",
                ServiceName = "Overnight Delivery",
                Rate = _settings.OvernightRate,
                MinDays = 1,
                MaxDays = 1,
                Description = $"Next Day Delivery — {_settings.OvernightRate:C}"
            }
        };

        return Task.FromResult(ApiResponse<IEnumerable<ShippingRateDto>>.Ok(rates));
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentAsync(int orderId, string userId, string userRole)
    {
        var order = userRole == Roles.Admin
            ? await _orderRepository.GetByIdAsync(orderId)
            : await _orderRepository.GetByIdForUserAsync(orderId, userId);

        if (order is null) return ApiResponse<ShipmentDto>.Fail("Order not found.");

        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId);
        if (shipment is null) return ApiResponse<ShipmentDto>.Fail("No shipment information available yet.");

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment));
    }

    public async Task<ApiResponse<ShipmentDto>> UpdateTrackingAsync(int orderId, UpdateTrackingDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null) return ApiResponse<ShipmentDto>.Fail("Order not found.");

        if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing &&
            order.Status != OrderStatus.Shipped)
            return ApiResponse<ShipmentDto>.Fail("Order must be Confirmed, Processing, or Shipped to add tracking.");

        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId);

        if (shipment is null)
        {
            shipment = new Shipment
            {
                OrderId = orderId,
                Carrier = dto.Carrier,
                TrackingNumber = dto.TrackingNumber,
                TrackingUrl = dto.TrackingUrl,
                Status = ShipmentStatus.Shipped,
                ShippedAt = DateTime.UtcNow,
                EstimatedDelivery = dto.EstimatedDelivery,
                Notes = dto.Notes
            };
            await _shipmentRepository.CreateAsync(shipment);
        }
        else
        {
            shipment.Carrier = dto.Carrier;
            shipment.TrackingNumber = dto.TrackingNumber;
            shipment.TrackingUrl = dto.TrackingUrl;
            if (dto.EstimatedDelivery.HasValue) shipment.EstimatedDelivery = dto.EstimatedDelivery;
            if (dto.Notes is not null) shipment.Notes = dto.Notes;
            if (shipment.Status == ShipmentStatus.Pending) shipment.Status = ShipmentStatus.Shipped;
            await _shipmentRepository.UpdateAsync(shipment);
        }

        // Advance order status to Shipped
        if (order.Status != OrderStatus.Shipped)
        {
            order.Status = OrderStatus.Shipped;
            await _orderRepository.UpdateAsync(order);
        }

        _logger.LogInformation("Tracking updated for order {OrderId}: {Carrier} {Tracking}",
            orderId, dto.Carrier, dto.TrackingNumber);

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Tracking information updated.");
    }

    public async Task<ApiResponse<ShipmentDto>> UpdateStatusAsync(int orderId, UpdateShipmentStatusDto dto)
    {
        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId);
        if (shipment is null) return ApiResponse<ShipmentDto>.Fail("Shipment not found.");

        shipment.Status = dto.Status;
        if (dto.Notes is not null) shipment.Notes = dto.Notes;

        if (dto.Status == ShipmentStatus.Delivered)
            shipment.DeliveredAt = DateTime.UtcNow;

        await _shipmentRepository.UpdateAsync(shipment);

        // Sync order status on delivery
        if (dto.Status == ShipmentStatus.Delivered)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is not null)
            {
                order.Status = OrderStatus.Delivered;
                await _orderRepository.UpdateAsync(order);
            }
        }

        return ApiResponse<ShipmentDto>.Ok(MapToDto(shipment), "Shipment status updated.");
    }

    private static ShipmentDto MapToDto(Shipment s) => new()
    {
        Id = s.Id,
        OrderId = s.OrderId,
        Carrier = s.Carrier,
        TrackingNumber = s.TrackingNumber,
        TrackingUrl = s.TrackingUrl,
        Status = s.Status,
        ShippedAt = s.ShippedAt,
        EstimatedDelivery = s.EstimatedDelivery,
        DeliveredAt = s.DeliveredAt,
        Notes = s.Notes
    };
}
