namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record FeeSummaryDTO(int StudentId, decimal? TotalFeeAmount, decimal TotalPaid, decimal PendingAmount, IReadOnlyList<FeePaymentResponseDTO> Payments, IReadOnlyList<FeeComponentDTO> FeeComponents);
