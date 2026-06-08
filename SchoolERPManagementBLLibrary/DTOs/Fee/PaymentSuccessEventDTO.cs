namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record PaymentSuccessEventDTO(int StudentId, int FeeStructureId, decimal AmountPaid, string TransactionId, string PaymentMethod);
