using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Order;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> GetOrderAsync(int orderId, string userId, string userRole);
    Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetMyOrdersAsync(string userId, int page, int pageSize);
    Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetAllOrdersAsync(int page, int pageSize, OrderStatus? status);
    Task<ApiResponse<OrderDto>> PlaceOrderAsync(string userId, PlaceOrderDto dto);
    Task<ApiResponse> CancelOrderAsync(int orderId, string userId, string userRole, string? reason);
    Task<ApiResponse<OrderDto>> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto);
}
