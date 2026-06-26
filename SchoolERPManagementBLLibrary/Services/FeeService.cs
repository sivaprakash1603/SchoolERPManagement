using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Stripe;
using Stripe.Checkout;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class FeeService : IFeeService
{
    private readonly IRepository<int, Feepayment> _feePaymentRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Feestructure> _feeStructureRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public FeeService(
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Feestructure> feeStructureRepository,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _feePaymentRepository = feePaymentRepository;
        _studentRepository = studentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _feeStructureRepository = feeStructureRepository;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<FeeStructureResponseDTO> AddFeeStructureAsync(AddFeeStructureDTO dto, CancellationToken cancellationToken)
    {
        var feeStructure = new Feestructure
        {
            Classid = dto.ClassId,
            Academicyearid = dto.AcademicYearId,
            Feename = dto.FeeName,
            Totalamount = dto.TotalAmount,
            Duedate = dto.DueDate
        };

        await _feeStructureRepository.AddAsync(feeStructure, save: true, ct: cancellationToken);
        return new FeeStructureResponseDTO(feeStructure.Id, feeStructure.Classid, feeStructure.Academicyearid, feeStructure.Feename, feeStructure.Totalamount, feeStructure.Duedate);
    }

    public async Task<FeePaymentResponseDTO> PayFeesAsync(FeePaymentDTO dto, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var feeStructure = await _feeStructureRepository.GetByIdAsync(dto.FeeStructureId);
        if (feeStructure is null)
        {
            throw new EntityNotFoundException("Fee Structure", dto.FeeStructureId.ToString());
        }

        var payments = await _feePaymentRepository.Query(true)
            .Where(payment => payment.Studentid == dto.StudentId && payment.Feestructureid == dto.FeeStructureId)
            .ToListAsync(cancellationToken);

        var totalPaid = payments.Sum(payment => payment.Amountpaid);
        var remainingAmount = feeStructure.Totalamount - totalPaid;

        if (dto.AmountPaid > remainingAmount)
        {
            throw new BusinessRuleException($"Payment amount of {dto.AmountPaid} exceeds the remaining balance of {remainingAmount}.");
        }

        var payment = new Feepayment
        {
            Studentid = dto.StudentId,
            Feestructureid = dto.FeeStructureId,
            Amountpaid = dto.AmountPaid,
            Paymentdate = dto.PaymentDate ?? DateTime.UtcNow,
            Paymentmethod = dto.PaymentMethod?.ToLower(),
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
        var student = await _studentRepository.GetByIdAsync(dto.StudentId);
        if (student is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var feeStructure = await _feeStructureRepository.GetByIdAsync(dto.FeeStructureId);
        if (feeStructure is null)
        {
            throw new EntityNotFoundException("FeeStructure", dto.FeeStructureId.ToString());
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" }, // Note: UPI requires IN region. Since stripe config might not support upi without proper setup, we'll enable Automatic Payment Methods later if needed. For now we will add 'upi'. Wait, wait. I will just add "upi" to the list as the user asked.
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
                            Name = $"{feeStructure.Feename} for {student.Name}",
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
                { "StudentId", dto.StudentId.ToString() },
                { "FeeStructureId", dto.FeeStructureId.ToString() }
            }
        };
        options.PaymentMethodTypes.Add("upi");

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
                    var transactionId = session.PaymentIntentId ?? session.Id;

                    int feeStructureId = 0;
                    if (session.Metadata.TryGetValue("FeeStructureId", out var feeStructureIdString))
                    {
                        int.TryParse(feeStructureIdString, out feeStructureId);
                    }

                    // Prevent duplicate entries if the webhook is delivered multiple times
                    bool paymentExists = await _feePaymentRepository.Query(false)
                        .AnyAsync(p => p.Transactionid == transactionId, cancellationToken);

                    if (paymentExists)
                    {
                        return;
                    }

                    var payment = new Feepayment
                    {
                        Studentid = studentId,
                        Feestructureid = feeStructureId,
                        Amountpaid = (decimal)(session.AmountTotal ?? 0) / 100m,
                        Paymentdate = DateTime.UtcNow,
                        Paymentmethod = "Stripe",
                        Transactionid = transactionId
                    };

                    await _feePaymentRepository.AddAsync(payment, save: true, ct: cancellationToken);

                    try
                    {
                        var pdfBytes = await GenerateReceiptPdfAsync(transactionId, cancellationToken);
                        
                        var studentWithParent = await _studentRepository.Query(true)
                            .Include(s => s.Studentparents)
                            .ThenInclude(sp => sp.Parent)
                            .ThenInclude(p => p.User)
                            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

                        if (studentWithParent != null)
                        {
                            foreach (var sp in studentWithParent.Studentparents)
                            {
                                if (!string.IsNullOrEmpty(sp.Parent?.User?.Email))
                                {
                                    string emailBody = $@"
                                    <h3>Payment Successful</h3>
                                    <p>Dear Parent,</p>
                                    <p>We have successfully received the fee payment of {payment.Amountpaid.ToString("C", new System.Globalization.CultureInfo("en-IN"))} for {studentWithParent.Name}.</p>
                                    <p>Please find the payment receipt attached to this email.</p>
                                    <p>Thank you,<br/>School ERP Management</p>";

                                    await _emailService.SendEmailWithAttachmentAsync(
                                        sp.Parent.User.Email,
                                        $"Fee Payment Receipt - {studentWithParent.Name}",
                                        emailBody,
                                        pdfBytes,
                                        $"Receipt_{transactionId}.pdf",
                                        cancellationToken
                                    );
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send receipt email: {ex.Message}");
                    }
                }
            }
        }
        catch (StripeException e)
        {
            throw new BusinessRuleException($"Stripe Webhook Error: {e.Message}");
        }
    }

    public async Task<IReadOnlyList<ClassFeeSummaryDTO>> GetClassFeeSummariesAsync(int classId, int academicYearId, CancellationToken cancellationToken)
    {
        var enrollments = await _studentEnrollmentRepository.Query(true)
            .Include(e => e.Student).ThenInclude(s => s.User)
            .Where(e => e.Classid == classId && e.Academicyearid == academicYearId)
            .ToListAsync(cancellationToken);

        var students = enrollments.Select(e => e.Student).Where(s => s != null).ToList();
        var studentIds = students.Select(s => s.Id).ToList();

        var feeStructures = await _feeStructureRepository.Query(true)
            .Where(f => f.Classid == classId && f.Academicyearid == academicYearId)
            .ToListAsync(cancellationToken);

        decimal totalFeeAmount = feeStructures.Sum(f => f.Totalamount);

        var payments = await _feePaymentRepository.Query(true)
            .Where(p => studentIds.Contains(p.Studentid))
            .ToListAsync(cancellationToken);

        var paymentsByStudent = payments.GroupBy(p => p.Studentid)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amountpaid));

        var summaries = students.Select(student =>
        {
            paymentsByStudent.TryGetValue(student.Id, out decimal paid);
            decimal pending = Math.Max(totalFeeAmount - paid, 0m);

            return new ClassFeeSummaryDTO(
                student.Id,
                student.Name ?? "Unknown Student",
                student.Regno ?? "N/A",
                totalFeeAmount,
                paid,
                pending
            );
        }).ToList();

        return summaries;
    }

    public async Task<CheckoutSessionResultDTO> GetCheckoutSessionDetailsAsync(string sessionId, CancellationToken cancellationToken)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);

        string studentName = "Unknown Student";
        string feeName = "Unknown Fee";
        decimal amountPaid = (decimal)(session.AmountTotal ?? 0) / 100m;
        string transactionId = session.PaymentIntentId ?? session.Id;

        if (session.Metadata.TryGetValue("StudentId", out var studentIdString) && int.TryParse(studentIdString, out int studentId))
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student != null)
            {
                studentName = student.Name;
            }
        }

        if (session.Metadata.TryGetValue("FeeStructureId", out var feeStructureIdString) && int.TryParse(feeStructureIdString, out int feeStructureId))
        {
            var feeStructure = await _feeStructureRepository.GetByIdAsync(feeStructureId);
            if (feeStructure != null)
            {
                feeName = feeStructure.Feename;
            }
        }

        return new CheckoutSessionResultDTO(
            transactionId,
            amountPaid,
            studentName,
            feeName,
            DateTime.UtcNow
        );
    }

    public async Task<byte[]> GenerateReceiptPdfAsync(string transactionId, CancellationToken cancellationToken)
    {
        // Find the payment record by transaction ID
        var payment = await _feePaymentRepository.Query(true)
            .Include(p => p.Student)
            .Include(p => p.Feestructure)
            .FirstOrDefaultAsync(p => p.Transactionid == transactionId, cancellationToken);

        if (payment == null)
        {
            throw new EntityNotFoundException("Fee Payment Transaction", transactionId);
        }

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, payment));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("School ERP Management").FontSize(20).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                column.Item().Text("Payment Receipt").FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
            });
        });
    }

    void ComposeContent(QuestPDF.Infrastructure.IContainer container, Feepayment payment)
    {
        container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Text($"Receipt No: {payment.Id}");
            column.Item().Text($"Transaction ID: {payment.Transactionid}");
            column.Item().Text($"Payment Date: {payment.Paymentdate?.ToString("f") ?? "N/A"}");
            column.Item().Text($"Payment Method: {payment.Paymentmethod}");
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Student Details").SemiBold();
                    col.Item().Text($"Name: {payment.Student?.Name ?? "N/A"}");
                    col.Item().Text($"Reg No: {payment.Student?.Regno ?? "N/A"}");
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Fee Details").SemiBold();
                    col.Item().Text($"Fee Name: {payment.Feestructure?.Feename ?? "General Fee"}");
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Description").SemiBold();
                    header.Cell().AlignRight().Text("Amount Paid (INR)").SemiBold();
                });

                table.Cell().Text(payment.Feestructure?.Feename ?? "Fee Payment");
                table.Cell().AlignRight().Text(payment.Amountpaid.ToString("C", new System.Globalization.CultureInfo("en-IN")));
            });

            column.Item().PaddingTop(20).AlignRight().Text($"Total Paid: {payment.Amountpaid.ToString("C", new System.Globalization.CultureInfo("en-IN"))}").FontSize(14).SemiBold();
        });
    }

    void ComposeFooter(QuestPDF.Infrastructure.IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }
}
