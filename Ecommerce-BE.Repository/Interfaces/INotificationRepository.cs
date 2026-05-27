using Ecommerce_BE.Shared.Kernel.Common;
using Ecommerce_BE.Shared.Kernel.Models;

namespace Ecommerce_BE.Repository.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id);
    Task<PagedResult<Notification>> GetByUserIdAsync(string userId, int page, int pageSize, bool? isRead = null);
    Task<int> GetUnreadCountAsync(string userId);
    Task AddAsync(Notification notification);
    Task MarkAsReadAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteAsync(Notification notification);
    Task SaveAsync();
}
