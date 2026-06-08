using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IRepository<int, Notification> _notificationRepository;
    private readonly IRepository<int, Usernotification> _userNotificationRepository;
    private readonly IRepository<int, User> _userRepository;

    public NotificationService(
        IRepository<int, Notification> notificationRepository,
        IRepository<int, Usernotification> userNotificationRepository,
        IRepository<int, User> userRepository)
    {
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _userRepository = userRepository;
    }

    public async Task<NotificationResponseDTO> SendNotificationAsync(SendNotificationDTO dto, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Title = dto.Title,
            Message = dto.Message,
            Createdbyuserid = dto.CreatedByUserId,
            Createdat = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, save: true, ct: cancellationToken);

        foreach (var userId in dto.TargetUserIds.Distinct())
        {
            if (await _userRepository.GetByIdAsync(userId) is null)
            {
                continue;
            }

            var userNotification = new Usernotification
            {
                Userid = userId,
                Notificationid = notification.Id,
                Isread = false
            };

            await _userNotificationRepository.AddAsync(userNotification, save: true, ct: cancellationToken);
        }

        return new NotificationResponseDTO(notification.Id, notification.Title, notification.Message, notification.Createdbyuserid, notification.Createdat);
    }

    public async Task<IReadOnlyList<UserNotificationResponseDTO>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken)
    {
        return await _userNotificationRepository.Query(true)
            .Where(userNotification => userNotification.Userid == userId)
            .Include(userNotification => userNotification.Notification)
            .OrderByDescending(userNotification => userNotification.Notification.Createdat)
            .Select(userNotification => new UserNotificationResponseDTO(
                userNotification.Id,
                userNotification.Notificationid,
                userNotification.Notification.Title,
                userNotification.Notification.Message,
                userNotification.Notification.Createdat,
                userNotification.Isread))
            .ToListAsync(cancellationToken);
    }
}
