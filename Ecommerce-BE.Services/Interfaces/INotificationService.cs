using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Notification;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Services.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetMyNotificationsAsync(string userId, int page, int pageSize, bool? isRead);
    Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
    Task<ApiResponse> MarkAsReadAsync(int id, string userId);
    Task<ApiResponse> MarkAllAsReadAsync(string userId);
    Task<ApiResponse> DeleteAsync(int id, string userId);
    Task SendAsync(string userId, string title, string message, NotificationType type, string? relatedEntityType = null, string? relatedEntityId = null);
    Task SendToAllAdminsAsync(string title, string message, NotificationType type);
}
