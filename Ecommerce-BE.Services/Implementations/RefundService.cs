using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Refund;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce_BE.Services.Implementations;

public class RefundService : IRefundService
{
    private readonly IRefundRepository _refundRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ILogger<RefundService> _logger;

    public RefundService(
        IRefundRepository refundRepository,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IPaymentGatewayFactory gatewayFactory,
        ILogger<RefundService> logger)
    {
        _refundRepository = refundRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _gatewayFactory = gatewayFactory;
        _logger = logger;
    }

    public async Task<ApiResponse<RefundDto>> RequestRefundAsync(string userId, RequestRefundDto dto)
    {
        var order = await _orderRepository.GetByIdForUserAsync(dto.OrderId, userId);
        if (order is null) return ApiResponse<RefundDto>.Fail("Order not found.");

        if (order.Status != OrderStatus.Delivered)
            return ApiResponse<RefundDto>.Fail("Refunds can only be requested for delivered orders.");

        var payment = await _paymentRepository.GetByOrderIdAsync(dto.OrderId);
        if (payment is null || payment.Status != PaymentStatus.Paid)
            return ApiResponse<RefundDto>.Fail("No successful payment found for this order.");

        var alreadyRefunded = await _refundRepository.GetTotalRefundedAsync(dto.OrderId);
        var maxRefundable = order.TotalAmount - alreadyRefunded;

        if (dto.Amount > maxRefundable)
            return ApiResponse<RefundDto>.Fail($"Refund amount exceeds refundable amount of {maxRefundable:C}.");

        var refund = new Refund
        {
            OrderId = dto.OrderId,
            PaymentId = payment.Id,
            Amount = dto.Amount,
            Reason = dto.Reason,
            Status = RefundStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        await _refundRepository.CreateAsync(refund);
        _logger.LogInformation("Refund requested for order {OrderId}, amount={Amount}", dto.OrderId, dto.Amount);

        return ApiResponse<RefundDto>.Ok(MapToDto(refund), "Refund request submitted.");
    }

    public async Task<ApiResponse<IEnumerable<RefundDto>>> GetMyRefundsAsync(string userId)
    {
        var refunds = await _refundRepository.GetByUserIdAsync(userId);
        return ApiResponse<IEnumerable<RefundDto>>.Ok(refunds.Select(MapToDto));
    }

    public async Task<ApiResponse<PagedResult<RefundDto>>> GetAllRefundsAsync(
        int page, int pageSize, RefundStatus? status)
    {
        var (refunds, total) = await _refundRepository.GetAllAsync(page, pageSize, status);
        return ApiResponse<PagedResult<RefundDto>>.Ok(new PagedResult<RefundDto>
        {
            Items = refunds.Select(MapToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<RefundDto>> GetByIdAsync(int id, string userId, string userRole)
    {
        var refund = await _refundRepository.GetByIdAsync(id);
        if (refund is null) return ApiResponse<RefundDto>.Fail("Refund not found.");

        if (userRole != Roles.Admin && refund.Order.UserId != userId)
            return ApiResponse<RefundDto>.Fail("Refund not found.");

        return ApiResponse<RefundDto>.Ok(MapToDto(refund));
    }

    public async Task<ApiResponse<RefundDto>> ProcessRefundAsync(int id, ProcessRefundDto dto)
    {
        var refund = await _refundRepository.GetByIdAsync(id);
        if (refund is null) return ApiResponse<RefundDto>.Fail("Refund not found.");

        if (refund.Status == RefundStatus.Completed)
            return ApiResponse<RefundDto>.Fail("This refund has already been completed.");

        if (dto.Status == RefundStatus.Processing || dto.Status == RefundStatus.Completed)
        {
            var payment = await _paymentRepository.GetByIdAsync(refund.PaymentId ?? 0);
            if (payment?.GatewayPaymentId is not null && dto.Status == RefundStatus.Completed)
            {
                var gateway = _gatewayFactory.GetGateway(payment.Gateway);
                var result = await gateway.RefundAsync(payment.GatewayPaymentId, refund.Amount);

                if (!result.Success)
                    return ApiResponse<RefundDto>.Fail(result.Error ?? "Gateway refund failed.");

                refund.GatewayRefundId = result.GatewayRefundId;
            }
        }

        refund.Status = dto.Status;
        if (dto.Notes is not null) refund.Notes = dto.Notes;
        if (dto.Status == RefundStatus.Completed) refund.ProcessedAt = DateTime.UtcNow;

        await _refundRepository.UpdateAsync(refund);

        // Update order payment status
        if (dto.Status == RefundStatus.Completed)
        {
            var order = await _orderRepository.GetByIdAsync(refund.OrderId);
            if (order is not null)
            {
                var totalRefunded = await _refundRepository.GetTotalRefundedAsync(refund.OrderId);
                order.PaymentStatus = totalRefunded >= order.TotalAmount
                    ? PaymentStatus.Refunded
                    : PaymentStatus.PartiallyRefunded;

                if (order.PaymentStatus == PaymentStatus.Refunded)
                    order.Status = OrderStatus.Refunded;

                await _orderRepository.UpdateAsync(order);
            }
        }

        _logger.LogInformation("Refund {Id} status updated to {Status}", id, dto.Status);
        return ApiResponse<RefundDto>.Ok(MapToDto(refund), "Refund updated.");
    }

    private static RefundDto MapToDto(Refund r) => new()
    {
        Id = r.Id,
        OrderId = r.OrderId,
        OrderNumber = r.Order?.OrderNumber ?? string.Empty,
        Amount = r.Amount,
        Reason = r.Reason,
        Status = r.Status,
        GatewayRefundId = r.GatewayRefundId,
        Notes = r.Notes,
        RequestedAt = r.RequestedAt,
        ProcessedAt = r.ProcessedAt
    };
}
