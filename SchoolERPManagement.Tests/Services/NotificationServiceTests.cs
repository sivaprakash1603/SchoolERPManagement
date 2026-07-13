using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;
using MockQueryable.Moq;

namespace SchoolERPManagement.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IRepository<int, Notification>> _notificationRepoMock;
    private readonly Mock<IRepository<int, Usernotification>> _userNotificationRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly Mock<INotificationPusher> _notificationPusherMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _notificationRepoMock = new Mock<IRepository<int, Notification>>();
        _userNotificationRepoMock = new Mock<IRepository<int, Usernotification>>();
        _userRepoMock = new Mock<IRepository<int, User>>();
        _notificationPusherMock = new Mock<INotificationPusher>();
        _service = new NotificationService(
            _notificationRepoMock.Object,
            _userNotificationRepoMock.Object,
            _userRepoMock.Object,
            _notificationPusherMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task SendNotificationAsync_ValidData_ShouldAddAndPush()
    {
        var targetUserIds = new List<int> { 1, 2, 3 }; // User 3 does not exist
        var dto = new SendNotificationDTO("Test Title", "Test Message", 99, targetUserIds);

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
        _userRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((User)null);

        var result = await _service.SendNotificationAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Title");

        _notificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _userNotificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Usernotification>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _notificationPusherMock.Verify(p => p.PushNotificationAsync(It.IsAny<int>(), It.IsAny<UserNotificationResponseDTO>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnNotifications()
    {
        var items = new List<Usernotification>
        {
            new Usernotification { Id = 1, Userid = 1, Notificationid = 1, Isread = false, Notification = new Notification { Createdat = DateTime.UtcNow } },
            new Usernotification { Id = 2, Userid = 1, Notificationid = 2, Isread = true, Notification = new Notification { Createdat = DateTime.UtcNow.AddMinutes(-5) } }
        };

        _userNotificationRepoMock.Setup(r => r.Query(true)).Returns(items.BuildMockDbSet().Object);

        var result = await _service.GetUserNotificationsAsync(1, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task MarkAsReadAsync_Valid_ShouldReturnTrue()
    {
        var items = new List<Usernotification>
        {
            new Usernotification { Id = 1, Userid = 1, Notificationid = 1, Isread = false }
        };

        _userNotificationRepoMock.Setup(r => r.Query(true)).Returns(items.BuildMockDbSet().Object);

        var result = await _service.MarkAsReadAsync(1, 1, CancellationToken.None);

        result.Should().BeTrue();
        _userNotificationRepoMock.Verify(r => r.UpdateAsync(It.Is<Usernotification>(un => un.Isread == true), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotFound_ShouldReturnFalse()
    {
        _userNotificationRepoMock.Setup(r => r.Query(true)).Returns(new List<Usernotification>().BuildMockDbSet().Object);

        var result = await _service.MarkAsReadAsync(1, 99, CancellationToken.None);

        result.Should().BeFalse();
        _userNotificationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Usernotification>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_HasUnread_ShouldReturnTrueAndUpdate()
    {
        var items = new List<Usernotification>
        {
            new Usernotification { Id = 1, Userid = 1, Notificationid = 1, Isread = false },
            new Usernotification { Id = 2, Userid = 1, Notificationid = 2, Isread = false }
        };

        _userNotificationRepoMock.Setup(r => r.Query(true)).Returns(items.BuildMockDbSet().Object);

        var result = await _service.MarkAllAsReadAsync(1, CancellationToken.None);

        result.Should().BeTrue();
        _userNotificationRepoMock.Verify(r => r.UpdateAsync(It.Is<Usernotification>(un => un.Isread == true), false, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _userNotificationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_NoUnread_ShouldReturnTrueAndNotUpdate()
    {
        var items = new List<Usernotification>
        {
            new Usernotification { Id = 1, Userid = 1, Notificationid = 1, Isread = true }
        };

        _userNotificationRepoMock.Setup(r => r.Query(true)).Returns(items.BuildMockDbSet().Object);

        var result = await _service.MarkAllAsReadAsync(1, CancellationToken.None);

        result.Should().BeTrue();
        _userNotificationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Usernotification>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        _userNotificationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
