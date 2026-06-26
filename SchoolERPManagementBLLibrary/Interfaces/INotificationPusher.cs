using SchoolERPManagementBLLibrary.DTOs.Notification;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface INotificationPusher
{
    Task PushNotificationAsync(int userId, UserNotificationResponseDTO notification, CancellationToken cancellationToken);
}
