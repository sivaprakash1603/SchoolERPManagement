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

        [HttpPut("user/{userId}/read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int userId, int notificationId, CancellationToken cancellationToken)
        {
            var success = await _notificationService.MarkAsReadAsync(userId, notificationId, cancellationToken);
            if (!success) return NotFound("Notification not found or access denied.");
            return Ok(new { success = true });
        }

        [HttpPut("user/{userId}/readAll")]
        public async Task<IActionResult> MarkAllAsRead(int userId, CancellationToken cancellationToken)
        {
            var success = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return Ok(new { success = true });
        }
    }
}
