using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IRepository<int, Notification>> _notificationRepoMock;
    private readonly Mock<IRepository<int, Usernotification>> _userNotificationRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _notificationRepoMock = new Mock<IRepository<int, Notification>>();
        _userNotificationRepoMock = new Mock<IRepository<int, Usernotification>>();
        _userRepoMock = new Mock<IRepository<int, User>>();

        _notificationService = new NotificationService(
            _notificationRepoMock.Object,
            _userNotificationRepoMock.Object,
            _userRepoMock.Object
        ,
            new Moq.Mock<AutoMapper.IMapper>().Object
        );
    }

    [Fact]
    public async Task SendNotificationAsync_ValidData_ShouldCreateNotificationAndUserNotifications()
    {
        
        var targetUserIds = new List<int> { 2, 3 };
        var dto = new SendNotificationDTO("Important Update", "School closed tomorrow.", 1, targetUserIds);

        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
        _userRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new User { Id = 3 });

        
        var result = await _notificationService.SendNotificationAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Title.Should().Be("Important Update");

        _notificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _userNotificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Usernotification>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendNotificationAsync_InvalidTargetUser_ShouldSkipUser()
    {
        
        var targetUserIds = new List<int> { 2, 999 };
        var dto = new SendNotificationDTO("Important Update", "School closed tomorrow.", 1, targetUserIds);

        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
        _userRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        
        var result = await _notificationService.SendNotificationAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        _userNotificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Usernotification>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnUserNotifications()
    {
        
        var notification = new Notification { Id = 1, Title = "Update", Message = "Text", Createdat = DateTime.UtcNow };
        var userNotifications = new List<Usernotification>
        {
            new Usernotification { Id = 1, Userid = 2, Notificationid = 1, Notification = notification, Isread = false }
        };

        _userNotificationRepoMock.Setup(r => r.Query(true)).Returns(userNotifications.AsQueryable().BuildMock());

        
        var result = await _notificationService.GetUserNotificationsAsync(2, CancellationToken.None);

        
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Update");
    }
}
