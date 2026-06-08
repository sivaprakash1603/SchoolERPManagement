using SchoolERPManagementBLLibrary.DTOs.Fee;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IFeeService
{
    Task<FeePaymentResponseDTO> PayFeesAsync(FeePaymentDTO dto, CancellationToken cancellationToken);
    Task<FeeSummaryDTO> GetFeeDetailsAsync(int studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FeePaymentResponseDTO>> GetPaymentHistoryAsync(int studentId, CancellationToken cancellationToken);
    Task<string> CreateStripeCheckoutSessionAsync(CreateCheckoutSessionDTO dto, CancellationToken cancellationToken);
    Task HandleStripeWebhookAsync(string json, string signature, CancellationToken cancellationToken);
    Task<FeeStructureResponseDTO> AddFeeStructureAsync(AddFeeStructureDTO dto, CancellationToken cancellationToken);
}
