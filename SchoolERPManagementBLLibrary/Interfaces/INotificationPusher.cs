using SchoolERPManagementBLLibrary.DTOs.Notification;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface INotificationPusher
{
    Task PushNotificationAsync(int userId, NotificationResponseDTO notification, CancellationToken cancellationToken);
}
