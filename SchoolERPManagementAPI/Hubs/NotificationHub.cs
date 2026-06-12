using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SchoolERPManagementAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // Clients will listen for the "ReceiveNotification" event.
    // The server will push messages to connected clients automatically.
}
