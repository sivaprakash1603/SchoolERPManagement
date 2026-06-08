using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class FeeServiceTests
{
    private readonly Mock<IRepository<int, Feepayment>> _feePaymentRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _studentEnrollmentRepoMock;
    private readonly Mock<IRepository<int, Feestructure>> _feeStructureRepoMock;
    private readonly Mock<IPaymentGatewayService> _paymentGatewayMock;
    private readonly FeeService _feeService;

    public FeeServiceTests()
    {
        _feePaymentRepoMock = new Mock<IRepository<int, Feepayment>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _studentEnrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _feeStructureRepoMock = new Mock<IRepository<int, Feestructure>>();
        _paymentGatewayMock = new Mock<IPaymentGatewayService>();

        _feeService = new FeeService(
            _feePaymentRepoMock.Object,
            _studentRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _feeStructureRepoMock.Object,
            _paymentGatewayMock.Object
        );
    }

    [Fact]
    public async Task PayFeesAsync_ValidData_ShouldCreatePayment()
    {
        // Arrange
        var dto = new FeePaymentDTO(1, 1, 5000m, DateTime.Parse("2025-01-01T10:00:00Z"), "Credit Card", "TXN123");
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        // Act
        var result = await _feeService.PayFeesAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AmountPaid.Should().Be(5000m);
        result.TransactionId.Should().Be("TXN123");

        _feePaymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Feepayment>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PayFeesAsync_InvalidStudent_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new FeePaymentDTO(999, 1, 5000m, DateTime.Parse("2025-01-01T10:00:00Z"), "Credit Card", "TXN123");
        _studentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Student?)null);

        // Act
        Func<Task> action = async () => await _feeService.PayFeesAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Student with identifier '999' was not found.");
    }

    [Fact]
    public async Task GetFeeDetailsAsync_ShouldReturnFeeSummary()
    {
        // Arrange
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        var payments = new List<Feepayment>
        {
            new Feepayment { Id = 1, Studentid = 1, Feestructureid = 1, Amountpaid = 1000m, Paymentdate = DateTime.Parse("2025-01-01T10:00:00Z") }
        };
        _feePaymentRepoMock.Setup(r => r.Query(true)).Returns(payments.AsQueryable().BuildMock());

        var enrollment = new Studentenrollment { Id = 1, Studentid = 1, Classid = 1, Academicyearid = 1 };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.AsQueryable().BuildMock());

        var feeStructures = new List<Feestructure>
        {
            new Feestructure { Id = 1, Classid = 1, Academicyearid = 1, Feename = "Tuition", Totalamount = 5000m }
        };
        _feeStructureRepoMock.Setup(r => r.Query(true)).Returns(feeStructures.AsQueryable().BuildMock());

        // Act
        var result = await _feeService.GetFeeDetailsAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalPaid.Should().Be(1000m);
        result.TotalFeeAmount.Should().Be(5000m);
        result.PendingAmount.Should().Be(4000m);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ShouldReturnPayments()
    {
        // Arrange
        var payments = new List<Feepayment>
        {
            new Feepayment { Id = 1, Studentid = 1, Feestructureid = 1, Amountpaid = 1000m, Paymentdate = DateTime.Parse("2025-01-01T10:00:00Z") }
        };
        _feePaymentRepoMock.Setup(r => r.Query(true)).Returns(payments.AsQueryable().BuildMock());

        // Act
        var result = await _feeService.GetPaymentHistoryAsync(1, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().AmountPaid.Should().Be(1000m);
    }

    [Fact]
    public async Task CreateStripeCheckoutSessionAsync_ValidData_ShouldReturnSessionUrl()
    {
        // Arrange
        var dto = new CreateCheckoutSessionDTO(1, 1, 5000m, "http://success", "http://cancel");
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        _paymentGatewayMock.Setup(p => p.CreateCheckoutSessionAsync(
                dto.Amount, "inr", $"School Fee Payment for Student ID: {dto.StudentId}", dto.StudentId, dto.FeeStructureId, dto.SuccessUrl, dto.CancelUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://stripe.com/session_url");

        // Act
        var result = await _feeService.CreateStripeCheckoutSessionAsync(dto, CancellationToken.None);

        // Assert
        result.Should().Be("http://stripe.com/session_url");
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ValidPayment_ShouldAddPayment()
    {
        // Arrange
        var json = "{}";
        var signature = "sig";
        var paymentEvent = new PaymentSuccessEventDTO(1, 1, 5000m, "TXN123", "Stripe");

        _paymentGatewayMock.Setup(p => p.ParseWebhookEventAsync(json, signature)).ReturnsAsync(paymentEvent);

        // Act
        await _feeService.HandleStripeWebhookAsync(json, signature, CancellationToken.None);

        // Assert
        _feePaymentRepoMock.Verify(r => r.AddAsync(It.Is<Feepayment>(f => 
            f.Studentid == 1 && f.Feestructureid == 1 && f.Amountpaid == 5000m && f.Transactionid == "TXN123" && f.Paymentmethod == "Stripe"), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddFeeStructureAsync_ValidData_ShouldReturnFeeStructure()
    {
        // Arrange
        var dto = new AddFeeStructureDTO(1, 1, "Tuition Fee", 5000m);

        // Act
        var result = await _feeService.AddFeeStructureAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ClassId.Should().Be(1);
        result.AcademicYearId.Should().Be(1);
        result.FeeName.Should().Be("Tuition Fee");
        result.TotalAmount.Should().Be(5000m);

        _feeStructureRepoMock.Verify(r => r.AddAsync(It.Is<Feestructure>(f => 
            f.Classid == 1 && f.Academicyearid == 1 && f.Feename == "Tuition Fee" && f.Totalamount == 5000m), true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
