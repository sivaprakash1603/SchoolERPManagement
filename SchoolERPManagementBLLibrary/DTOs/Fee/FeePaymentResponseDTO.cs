namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record FeePaymentResponseDTO(int Id, int StudentId, int FeeStructureId, decimal AmountPaid, DateTime? PaymentDate, string? PaymentMethod, string? TransactionId);
