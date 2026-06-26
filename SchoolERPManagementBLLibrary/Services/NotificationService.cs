using AutoMapper;
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
    private readonly INotificationPusher _notificationPusher;
    private readonly IMapper _mapper;

    public NotificationService(
        IRepository<int, Notification> notificationRepository,
        IRepository<int, Usernotification> userNotificationRepository,
        IRepository<int, User> userRepository,
        INotificationPusher notificationPusher,
        IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _userRepository = userRepository;
        _notificationPusher = notificationPusher;
        _mapper = mapper;
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

        var responseDto = _mapper.Map<NotificationResponseDTO>(notification);

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

            var userNotificationDto = new UserNotificationResponseDTO(
                userNotification.Id,
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Createdat,
                false
            );

            await _notificationPusher.PushNotificationAsync(userId, userNotificationDto, cancellationToken);
        }

        return responseDto;
    }

    public async Task<IReadOnlyList<UserNotificationResponseDTO>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken)
    {
        var items = await _userNotificationRepository.Query(true)
            .Where(userNotification => userNotification.Userid == userId)
            .Include(userNotification => userNotification.Notification)
            .OrderByDescending(userNotification => userNotification.Notification.Createdat)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<UserNotificationResponseDTO>>(items);
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId, CancellationToken cancellationToken)
    {
        var userNotification = await _userNotificationRepository.Query(true)
            .FirstOrDefaultAsync(un => un.Userid == userId && un.Notificationid == notificationId, cancellationToken);
            
        if (userNotification is null)
            return false;

        userNotification.Isread = true;
        await _userNotificationRepository.UpdateAsync(userNotification, save: true, ct: cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken)
    {
        var unreadNotifications = await _userNotificationRepository.Query(true)
            .Where(un => un.Userid == userId && un.Isread != true)
            .ToListAsync(cancellationToken);

        if (!unreadNotifications.Any())
            return true;

        foreach (var un in unreadNotifications)
        {
            un.Isread = true;
            await _userNotificationRepository.UpdateAsync(un, save: false, ct: cancellationToken);
        }
        
        await _userNotificationRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
