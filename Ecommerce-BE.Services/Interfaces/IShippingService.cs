using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Shipping;

namespace Ecommerce_BE.Services.Interfaces;

public interface IShippingService
{
    Task<ApiResponse<IEnumerable<ShippingRateDto>>> GetRatesAsync(GetShippingRatesDto dto);
    Task<ApiResponse<ShipmentDto>> GetShipmentAsync(int orderId, string userId, string userRole);
    Task<ApiResponse<ShipmentDto>> UpdateTrackingAsync(int orderId, UpdateTrackingDto dto);
    Task<ApiResponse<ShipmentDto>> UpdateStatusAsync(int orderId, UpdateShipmentStatusDto dto);
}
