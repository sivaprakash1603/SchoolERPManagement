namespace SchoolERPManagementBLLibrary.DTOs.Notification;

public record NotificationResponseDTO(int Id, string Title, string Message, int? CreatedByUserId, DateTime? CreatedAt);
