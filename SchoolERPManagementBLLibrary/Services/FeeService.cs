using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Stripe;
using Stripe.Checkout;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class FeeService : IFeeService
{
    private readonly IRepository<int, Feepayment> _feePaymentRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Feestructure> _feeStructureRepository;
    private readonly IConfiguration _configuration;

    public FeeService(
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Feestructure> feeStructureRepository,
        IConfiguration configuration)
    {
        _feePaymentRepository = feePaymentRepository;
        _studentRepository = studentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _feeStructureRepository = feeStructureRepository;
        _configuration = configuration;
    }

    public async Task<FeeStructureResponseDTO> AddFeeStructureAsync(AddFeeStructureDTO dto, CancellationToken cancellationToken)
    {
        var feeStructure = new Feestructure
        {
            Classid = dto.ClassId,
            Academicyearid = dto.AcademicYearId,
            Feename = dto.FeeName,
            Totalamount = dto.TotalAmount
        };

        await _feeStructureRepository.AddAsync(feeStructure, save: true, ct: cancellationToken);
        return new FeeStructureResponseDTO(feeStructure.Id, feeStructure.Classid, feeStructure.Academicyearid, feeStructure.Feename, feeStructure.Totalamount);
    }

    public async Task<FeePaymentResponseDTO> PayFeesAsync(FeePaymentDTO dto, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var payment = new Feepayment
        {
            Studentid = dto.StudentId,
            Feestructureid = dto.FeeStructureId,
            Amountpaid = dto.AmountPaid,
            Paymentdate = dto.PaymentDate ?? DateTime.Now,
            Paymentmethod = dto.PaymentMethod,
            Transactionid = dto.TransactionId
        };

        await _feePaymentRepository.AddAsync(payment, save: true, ct: cancellationToken);
        return new FeePaymentResponseDTO(payment.Id, payment.Studentid, payment.Feestructureid, payment.Amountpaid, payment.Paymentdate, payment.Paymentmethod, payment.Transactionid);
    }

    public async Task<FeeSummaryDTO> GetFeeDetailsAsync(int studentId, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(studentId) is null)
        {
            throw new EntityNotFoundException("Student", studentId.ToString());
        }

        var payments = await _feePaymentRepository.Query(true)
            .Where(payment => payment.Studentid == studentId)
            .OrderByDescending(payment => payment.Paymentdate)
            .Select(payment => new FeePaymentResponseDTO(payment.Id, payment.Studentid, payment.Feestructureid, payment.Amountpaid, payment.Paymentdate, payment.Paymentmethod, payment.Transactionid))
            .ToListAsync(cancellationToken);

        var totalPaid = payments.Sum(payment => payment.AmountPaid);

        var enrollment = await _studentEnrollmentRepository.Query(true)
            .Where(item => item.Studentid == studentId)
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        IReadOnlyList<FeeComponentDTO> components = Array.Empty<FeeComponentDTO>();
        decimal? totalFeeAmount = null;
        if (enrollment is not null)
        {
            // Get all fee components for the student's class and academic year
            var feeItems = await _feeStructureRepository.Query(true)
                .Where(fee => fee.Classid == enrollment.Classid && fee.Academicyearid == enrollment.Academicyearid)
                .OrderBy(fee => fee.Id)
                .Select(fee => new FeeComponentDTO(fee.Id, fee.Feename, fee.Totalamount))
                .ToListAsync(cancellationToken);

            components = feeItems;
            if (feeItems.Any())
            {
                totalFeeAmount = feeItems.Sum(fi => fi.Amount);
            }
        }

        var pendingAmount = totalFeeAmount.HasValue ? Math.Max(totalFeeAmount.Value - totalPaid, 0m) : 0m;
        return new FeeSummaryDTO(studentId, totalFeeAmount, totalPaid, pendingAmount, payments, components);
    }

    public async Task<IReadOnlyList<FeePaymentResponseDTO>> GetPaymentHistoryAsync(int studentId, CancellationToken cancellationToken)
    {
        return await _feePaymentRepository.Query(true)
            .Where(payment => payment.Studentid == studentId)
            .OrderByDescending(payment => payment.Paymentdate)
            .Select(payment => new FeePaymentResponseDTO(payment.Id, payment.Studentid, payment.Feestructureid, payment.Amountpaid, payment.Paymentdate, payment.Paymentmethod, payment.Transactionid))
            .ToListAsync(cancellationToken);
    }

    public async Task<string> CreateStripeCheckoutSessionAsync(CreateCheckoutSessionDTO dto, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(dto.Amount * 100), // Stripe expects amounts in cents/paise
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"School Fee Payment for Student ID: {dto.StudentId}",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = dto.SuccessUrl ?? "http://localhost:4200/payment-success",
            CancelUrl = dto.CancelUrl ?? "http://localhost:4200/payment-cancelled",
            Metadata = new Dictionary<string, string>
            {
                { "StudentId", dto.StudentId.ToString() }
            }
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return session.Url;
    }

    public async Task HandleStripeWebhookAsync(string json, string signature, CancellationToken cancellationToken)
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
                    var payment = new Feepayment
                    {
                        Studentid = studentId,
                        Feestructureid = 0, // Fallback since Stripe Webhook might not know
                        Amountpaid = (decimal)(session.AmountTotal ?? 0) / 100m,
                        Paymentdate = DateTime.Now,
                        Paymentmethod = "Stripe",
                        Transactionid = session.PaymentIntentId ?? session.Id
                    };

                    await _feePaymentRepository.AddAsync(payment, save: true, ct: cancellationToken);
                }
            }
        }
        catch (StripeException e)
        {
            throw new BusinessRuleException($"Stripe Webhook Error: {e.Message}");
        }
    }
}
