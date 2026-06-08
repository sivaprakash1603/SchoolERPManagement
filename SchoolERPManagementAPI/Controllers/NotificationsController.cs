using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDTO dto, CancellationToken cancellationToken)
        {
            var result = await _notificationService.SendNotificationAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId, CancellationToken cancellationToken)
        {
            var result = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            return Ok(result);
        }
    }
}
