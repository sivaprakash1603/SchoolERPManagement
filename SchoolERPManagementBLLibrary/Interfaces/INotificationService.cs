using SchoolERPManagementBLLibrary.DTOs.Notification;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface INotificationService
{
    Task<NotificationResponseDTO> SendNotificationAsync(SendNotificationDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserNotificationResponseDTO>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken);
}
