namespace SchoolERPManagementBLLibrary.DTOs.Notification;

public record UserNotificationResponseDTO(int Id, int NotificationId, string Title, string Message, DateTime? CreatedAt, bool? IsRead);
