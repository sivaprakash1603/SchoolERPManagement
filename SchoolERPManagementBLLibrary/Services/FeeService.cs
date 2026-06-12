using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace SchoolERPManagementBLLibrary.Services;

public sealed class FeeService : IFeeService
{
    private readonly IRepository<int, Feepayment> _feePaymentRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Feestructure> _feeStructureRepository;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IMapper _mapper;

    public FeeService(
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Feestructure> feeStructureRepository,
        IPaymentGatewayService paymentGatewayService,
        IMapper mapper)
    {
        _feePaymentRepository = feePaymentRepository;
        _studentRepository = studentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _feeStructureRepository = feeStructureRepository;
        _paymentGatewayService = paymentGatewayService;
        _mapper = mapper;
    }

    public async Task<FeePaymentResponseDTO> PayFeesAsync(FeePaymentDTO dto, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var feeDetails = await GetFeeDetailsAsync(dto.StudentId, cancellationToken);
        if (dto.AmountPaid > feeDetails.PendingAmount)
        {
            throw new BusinessRuleException($"Payment amount ({dto.AmountPaid}) exceeds pending fee amount ({feeDetails.PendingAmount}).");
        }

        var payment = new Feepayment
        {
            Studentid = dto.StudentId,
            Feestructureid = dto.FeeStructureId,
            Amountpaid = dto.AmountPaid,
            Paymentdate = dto.PaymentDate ?? DateTime.UtcNow,
            Paymentmethod = dto.PaymentMethod,
            Transactionid = dto.TransactionId
        };

        await _feePaymentRepository.AddAsync(payment, save: true, ct: cancellationToken);

        return _mapper.Map<FeePaymentResponseDTO>(payment);
    }

    public async Task<FeeSummaryDTO> GetFeeDetailsAsync(int studentId, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(studentId) is null)
        {
            throw new EntityNotFoundException("Student", studentId.ToString());
        }

        var paymentItems = await _feePaymentRepository.Query(true)
            .Where(payment => payment.Studentid == studentId)
            .OrderByDescending(payment => payment.Paymentdate)
            .ToListAsync(cancellationToken);
        var payments = _mapper.Map<IReadOnlyList<FeePaymentResponseDTO>>(paymentItems);

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
            var feeItemsList = await _feeStructureRepository.Query(true)
                .Where(fee => fee.Classid == enrollment.Classid && fee.Academicyearid == enrollment.Academicyearid)
                .OrderBy(fee => fee.Id)
                .ToListAsync(cancellationToken);
            var feeItems = _mapper.Map<IReadOnlyList<FeeComponentDTO>>(feeItemsList);

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
        var items = await _feePaymentRepository.Query(true)
            .Where(payment => payment.Studentid == studentId)
            .OrderByDescending(payment => payment.Paymentdate)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<FeePaymentResponseDTO>>(items);
    }

    public async Task<string> CreateStripeCheckoutSessionAsync(CreateCheckoutSessionDTO dto, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        return await _paymentGatewayService.CreateCheckoutSessionAsync(
            dto.Amount,
            "inr",
            $"School Fee Payment for Student ID: {dto.StudentId}",
            dto.StudentId,
            dto.FeeStructureId,
            dto.SuccessUrl,
            dto.CancelUrl,
            cancellationToken
        );
    }

    public async Task HandleStripeWebhookAsync(string json, string signature, CancellationToken cancellationToken)
    {
        var paymentEvent = await _paymentGatewayService.ParseWebhookEventAsync(json, signature);

        if (paymentEvent != null)
        {
            bool exists = await _feePaymentRepository.Query(true)
                .AnyAsync(p => p.Transactionid == paymentEvent.TransactionId, cancellationToken);
                
            if (exists)
            {
                return; // Idempotency check: ignore duplicate webhook events
            }

            var payment = new Feepayment
            {
                Studentid = paymentEvent.StudentId,
                Feestructureid = paymentEvent.FeeStructureId,
                Amountpaid = paymentEvent.AmountPaid,
                Paymentdate = DateTime.UtcNow,
                Paymentmethod = paymentEvent.PaymentMethod,
                Transactionid = paymentEvent.TransactionId
            };

            await _feePaymentRepository.AddAsync(payment, save: true, ct: cancellationToken);
        }
    }

    public async Task<FeeStructureResponseDTO> AddFeeStructureAsync(AddFeeStructureDTO dto, CancellationToken cancellationToken)
    {
        var structure = new Feestructure
        {
            Classid = dto.ClassId,
            Academicyearid = dto.AcademicYearId,
            Feename = dto.FeeName,
            Totalamount = dto.TotalAmount
        };

        await _feeStructureRepository.AddAsync(structure, save: true, ct: cancellationToken);

        return _mapper.Map<FeeStructureResponseDTO>(structure);
    }
}
