namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record CreateCheckoutSessionDTO(int StudentId, int FeeStructureId, decimal Amount, string? SuccessUrl, string? CancelUrl);
