using SchoolERPManagementBLLibrary.DTOs.Notification;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface INotificationService
{
    Task<NotificationResponseDTO> SendNotificationAsync(SendNotificationDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserNotificationResponseDTO>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken);
    Task<bool> MarkAsReadAsync(int userId, int notificationId, CancellationToken cancellationToken);
    Task<bool> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken);
}
