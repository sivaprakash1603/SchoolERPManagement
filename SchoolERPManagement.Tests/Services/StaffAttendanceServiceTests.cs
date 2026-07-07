using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class StaffAttendanceServiceTests
{
    private readonly Mock<IRepository<int, Staffattendance>> _staffAttendanceRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly StaffAttendanceService _staffAttendanceService;

    public StaffAttendanceServiceTests()
    {
        _staffAttendanceRepoMock = new Mock<IRepository<int, Staffattendance>>();
        _userRepoMock = new Mock<IRepository<int, User>>();

        _staffAttendanceService = new StaffAttendanceService(
            _staffAttendanceRepoMock.Object,
            _userRepoMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task MarkAttendanceAsync_ValidData_ShouldCreateAttendance()
    {
        
        var dto = new StaffAttendanceRequestDTO(1, DateOnly.Parse("2025-01-01"), "Present", "Daily", "On time");

        var user = new User { Id = 1, Username = "teacher_jdoe" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        
        _staffAttendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Staffattendance>().BuildMockDbSet().Object);

        
        var result = await _staffAttendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.Username.Should().Be("teacher_jdoe");
        result.Status.Should().Be("Present");

        _staffAttendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Staffattendance>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendanceAsync_InvalidUser_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new StaffAttendanceRequestDTO(999, DateOnly.Parse("2025-01-01"), "Present", "Daily", null);
        _userRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        
        Func<Task> action = async () => await _staffAttendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("User with identifier '999' was not found.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_DuplicateAttendance_ShouldUpdateAttendance()
    {
        // Arrange
        var dto = new StaffAttendanceRequestDTO(1, DateOnly.Parse("2025-01-01"), "Absent", "Daily", "Updated Remarks");

        var user = new User { Id = 1, Username = "teacher_jdoe" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var existingAttendance = new Staffattendance { Userid = 1, Date = DateOnly.Parse("2025-01-01"), Status = "present" };
        _staffAttendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Staffattendance> { existingAttendance }.BuildMockDbSet().Object);

        // Act
        var result = await _staffAttendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("absent");
        result.Remarks.Should().Be("Updated Remarks");

        _staffAttendanceRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Staffattendance>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAttendanceByUserAsync_ShouldReturnListOfAttendance()
    {
        
        var user = new User { Id = 1, Username = "teacher_jdoe" };
        var attendanceList = new List<Staffattendance>
        {
            new Staffattendance { Id = 1, Userid = 1, User = user, Date = DateOnly.Parse("2025-01-01") },
            new Staffattendance { Id = 2, Userid = 1, User = user, Date = DateOnly.Parse("2025-01-02") }
        };

        _staffAttendanceRepoMock.Setup(r => r.Query(true)).Returns(attendanceList.BuildMockDbSet().Object);

        
        var result = await _staffAttendanceService.GetAttendanceByUserAsync(1, CancellationToken.None);

        
        result.Should().HaveCount(2);
        result.First().Username.Should().Be("teacher_jdoe");
    }
}
