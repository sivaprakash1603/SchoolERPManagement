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

public class HomeworkDueReminderServiceTests
{
    private readonly Mock<IRepository<int, Homework>> _homeworkRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<HomeworkDueReminderService>> _loggerMock;
    private readonly HomeworkDueReminderService _service;

    public HomeworkDueReminderServiceTests()
    {
        _homeworkRepoMock = new Mock<IRepository<int, Homework>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<HomeworkDueReminderService>>();

        var serviceScopeMock = new Mock<IServiceScope>();
        var scopeServiceProviderMock = new Mock<IServiceProvider>();

        scopeServiceProviderMock.Setup(sp => sp.GetService(typeof(IRepository<int, Homework>)))
            .Returns(_homeworkRepoMock.Object);
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

        _service = new HomeworkDueReminderService(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendDueRemindersAsync_ShouldSendNotification_WhenHomeworkDueTomorrowAndNotSubmitted()
    {
        // Arrange
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var homework = new Homework
        {
            Id = 1,
            Title = "Math Assignment 1",
            Classid = 1,
            Duedate = tomorrow,
            Homeworksubmissions = new List<Homeworksubmission>()
        };

        var student = new Student
        {
            Id = 1,
            Userid = 10,
            Name = "Alice",
            Studentenrollments = new List<Studentenrollment>
            {
                new Studentenrollment { Classid = 1 }
            }
        };

        _homeworkRepoMock.Setup(r => r.Query(true))
            .Returns(new List<Homework> { homework }.BuildMockDbSet().Object);

        _studentRepoMock.Setup(r => r.Query(true))
            .Returns(new List<Student> { student }.BuildMockDbSet().Object);

        // Act
        await _service.SendDueRemindersAsync(CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(n => n.SendNotificationAsync(
            It.Is<SendNotificationDTO>(dto =>
                dto.Title == "Homework Deadline Reminder" &&
                dto.TargetUserIds.Contains(10)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
