using SchoolERPManagementBLLibrary.DTOs.Fee;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IPaymentGatewayService
{
    Task<string> CreateCheckoutSessionAsync(decimal amount, string currency, string productName, int studentId, int feeStructureId, string? successUrl, string? cancelUrl, CancellationToken cancellationToken);
    
    
    
    
    
    Task<PaymentSuccessEventDTO?> ParseWebhookEventAsync(string json, string signature);
}
