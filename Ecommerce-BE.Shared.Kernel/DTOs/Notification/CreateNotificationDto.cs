using System.ComponentModel.DataAnnotations;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Shared.Kernel.DTOs.Notification;

public class CreateNotificationDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.System;
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
}
