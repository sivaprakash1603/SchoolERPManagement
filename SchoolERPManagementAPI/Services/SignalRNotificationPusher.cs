using Microsoft.AspNetCore.SignalR;
using SchoolERPManagementAPI.Hubs;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Services;

public class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushNotificationAsync(int userId, UserNotificationResponseDTO notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}
