using Ecommerce_BE.Repository.Interfaces;
using Ecommerce_BE.Services.Interfaces;
using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.DTOs.Notification;
using Ecommerce_BE.Shared.Kernel.Models;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce_BE.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(INotificationRepository notifications, UserManager<ApplicationUser> userManager)
    {
        _notifications = notifications;
        _userManager = userManager;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetMyNotificationsAsync(string userId, int page, int pageSize, bool? isRead)
    {
        var result = await _notifications.GetByUserIdAsync(userId, page, pageSize, isRead);
        return ApiResponse<PagedResult<NotificationDto>>.Ok(result.Map(MapToDto));
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
    {
        var count = await _notifications.GetUnreadCountAsync(userId);
        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse> MarkAsReadAsync(int id, string userId)
    {
        var notification = await _notifications.GetByIdAsync(id);
        if (notification == null) return ApiResponse.Fail("Notification not found");
        if (notification.UserId != userId) return ApiResponse.Fail("Access denied");

        await _notifications.MarkAsReadAsync(id, userId);
        return ApiResponse.Ok("Marked as read");
    }

    public async Task<ApiResponse> MarkAllAsReadAsync(string userId)
    {
        await _notifications.MarkAllAsReadAsync(userId);
        return ApiResponse.Ok("All notifications marked as read");
    }

    public async Task<ApiResponse> DeleteAsync(int id, string userId)
    {
        var notification = await _notifications.GetByIdAsync(id);
        if (notification == null) return ApiResponse.Fail("Notification not found");
        if (notification.UserId != userId) return ApiResponse.Fail("Access denied");

        await _notifications.DeleteAsync(notification);
        await _notifications.SaveAsync();

        return ApiResponse.Ok("Notification deleted");
    }

    public async Task SendAsync(string userId, string title, string message, NotificationType type, string? relatedEntityType = null, string? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow
        };

        await _notifications.AddAsync(notification);
        await _notifications.SaveAsync();
    }

    public async Task SendToAllAdminsAsync(string title, string message, NotificationType type)
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        foreach (var admin in admins)
            await SendAsync(admin.Id, title, message, type);
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        IsRead = n.IsRead,
        RelatedEntityType = n.RelatedEntityType,
        RelatedEntityId = n.RelatedEntityId,
        CreatedAt = n.CreatedAt,
        ReadAt = n.ReadAt
    };
}
