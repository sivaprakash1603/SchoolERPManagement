using SchoolERPManagementBLLibrary.DTOs.Fee;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IPaymentGatewayService
{
    Task<string> CreateCheckoutSessionAsync(decimal amount, string currency, string productName, int studentId, int feeStructureId, string? successUrl, string? cancelUrl, CancellationToken cancellationToken);
    
    /// <summary>
    /// Parses a webhook event and returns a PaymentSuccessEventDTO if the event represents a successful payment.
    /// Returns null if the event is not a payment success event or is invalid.
    /// </summary>
    Task<PaymentSuccessEventDTO?> ParseWebhookEventAsync(string json, string signature);
}
