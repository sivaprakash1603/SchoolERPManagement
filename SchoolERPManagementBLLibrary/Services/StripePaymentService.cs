using Microsoft.Extensions.Configuration;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace SchoolERPManagementBLLibrary.Services;

public class StripePaymentService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;

    public StripePaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateCheckoutSessionAsync(decimal amount, string currency, string productName, int studentId, int feeStructureId, string? successUrl, string? cancelUrl, CancellationToken cancellationToken)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100), // Stripe expects amounts in cents/paise
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = productName,
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = successUrl ?? _configuration["Stripe:SuccessUrl"] ?? "http://localhost:4200/payment-success",
            CancelUrl = cancelUrl ?? _configuration["Stripe:CancelUrl"] ?? "http://localhost:4200/payment-cancelled",
            Metadata = new Dictionary<string, string>
            {
                { "StudentId", studentId.ToString() },
                { "FeeStructureId", feeStructureId.ToString() }
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return session.Url;
    }

    public Task<PaymentSuccessEventDTO?> ParseWebhookEventAsync(string json, string signature)
    {
        var endpointSecret = _configuration["Stripe:WebhookSecret"];
        
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, endpointSecret);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;

                if (session != null && session.Metadata.TryGetValue("StudentId", out var studentIdString) && int.TryParse(studentIdString, out int studentId))
                {
                    int feeStructureId = 0;
                    if (session.Metadata.TryGetValue("FeeStructureId", out var feeString) && int.TryParse(feeString, out int parsedFeeId))
                    {
                        feeStructureId = parsedFeeId;
                    }

                    var amountPaid = (decimal)(session.AmountTotal ?? 0) / 100m;
                    var transactionId = session.PaymentIntentId ?? session.Id;

                    return Task.FromResult<PaymentSuccessEventDTO?>(new PaymentSuccessEventDTO(studentId, feeStructureId, amountPaid, transactionId, "Stripe"));
                }
            }

            return Task.FromResult<PaymentSuccessEventDTO?>(null);
        }
        catch (StripeException e)
        {
            throw new BusinessRuleException($"Stripe Webhook Error: {e.Message}");
        }
    }
}
