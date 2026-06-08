namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record FeePaymentDTO(int StudentId, int FeeStructureId, decimal AmountPaid, DateTime? PaymentDate, string? PaymentMethod, string? TransactionId);
