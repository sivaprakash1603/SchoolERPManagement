namespace SchoolERPManagementBLLibrary.DTOs.Notification;

public record SendNotificationDTO(string Title, string Message, int? CreatedByUserId, IReadOnlyCollection<int> TargetUserIds);
