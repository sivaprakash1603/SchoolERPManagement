using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementAPI.Services;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class FeesDueReminderServiceTests
{
    private readonly Mock<IRepository<int, Feestructure>> _feeStructureRepoMock;
    private readonly Mock<IRepository<int, Feepayment>> _feePaymentRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<FeesDueReminderService>> _loggerMock;
    private readonly FeesDueReminderService _service;

    public FeesDueReminderServiceTests()
    {
        _feeStructureRepoMock = new Mock<IRepository<int, Feestructure>>();
        _feePaymentRepoMock = new Mock<IRepository<int, Feepayment>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<FeesDueReminderService>>();

        var serviceScopeMock = new Mock<IServiceScope>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();

        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(IRepository<int, Feestructure>)))
            .Returns(_feeStructureRepoMock.Object);
        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(IRepository<int, Feepayment>)))
            .Returns(_feePaymentRepoMock.Object);
        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(IRepository<int, Student>)))
            .Returns(_studentRepoMock.Object);
        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(INotificationService)))
            .Returns(_notificationServiceMock.Object);

        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(scopeServiceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        _service = new FeesDueReminderService(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendDueRemindersAsync_ShouldSendNotification_WhenFeesDueIn3DaysAndOutstanding()
    {
        // Arrange
        var dueTime = DateTime.UtcNow.AddDays(3).AddHours(-0.5); // Inside the threshold window

        var fee = new Feestructure
        {
            Id = 1,
            Feename = "Term 1 Tuition",
            Classid = 1,
            Totalamount = 5000m,
            Duedate = dueTime
        };

        var parent = new Parent
        {
            Id = 1,
            Userid = 20
        };

        var student = new Student
        {
            Id = 1,
            Userid = 10,
            Name = "Alice",
            Studentenrollments = new List<Studentenrollment>
            {
                new Studentenrollment { Classid = 1 }
            },
            Studentparents = new List<Studentparent>
            {
                new Studentparent { Parent = parent }
            }
        };

        _feeStructureRepoMock.Setup(r => r.Query(true))
            .Returns(new List<Feestructure> { fee }.BuildMockDbSet().Object);

        _studentRepoMock.Setup(r => r.Query(true))
            .Returns(new List<Student> { student }.BuildMockDbSet().Object);

        _feePaymentRepoMock.Setup(r => r.Query(true))
            .Returns(new List<Feepayment>().BuildMockDbSet().Object); // No payments yet

        // Act
        await _service.SendDueRemindersAsync(CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(n => n.SendNotificationAsync(
            It.Is<SendNotificationDTO>(dto =>
                dto.Title == "Fee Payment Due Reminder" &&
                dto.TargetUserIds.Contains(10) &&
                dto.TargetUserIds.Contains(20)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
