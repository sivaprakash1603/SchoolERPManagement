using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;
using MockQueryable.Moq;

namespace SchoolERPManagement.Tests.Services;

public class StaffAttendanceServiceTests
{
    private readonly Mock<IRepository<int, Staffattendance>> _attendanceRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly StaffAttendanceService _service;

    public StaffAttendanceServiceTests()
    {
        _attendanceRepoMock = new Mock<IRepository<int, Staffattendance>>();
        _userRepoMock = new Mock<IRepository<int, User>>();
        _service = new StaffAttendanceService(
            _attendanceRepoMock.Object,
            _userRepoMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task MarkAttendanceAsync_ValidNew_ShouldAdd()
    {
        var dto = new StaffAttendanceRequestDTO(1, DateOnly.FromDateTime(DateTime.UtcNow), "present", "Full day", "On time");
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Staffattendance>().BuildMockDbSet().Object);

        var result = await _service.MarkAttendanceAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("present");
        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Staffattendance>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendanceAsync_ValidExisting_ShouldUpdate()
    {
        var dto = new StaffAttendanceRequestDTO(1, DateOnly.FromDateTime(DateTime.UtcNow), "absent", "Full day", "Sick");
        var existing = new Staffattendance { Id = 1, Userid = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), Status = "present" };
        
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Staffattendance> { existing }.BuildMockDbSet().Object);

        var result = await _service.MarkAttendanceAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("absent");
        _attendanceRepoMock.Verify(r => r.UpdateAsync(existing, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendanceAsync_UserNotFound_ShouldThrowEntityNotFoundException()
    {
        var dto = new StaffAttendanceRequestDTO(99, DateOnly.FromDateTime(DateTime.UtcNow), "present", null, null);
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User)null);

        Func<Task> act = async () => await _service.MarkAttendanceAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task MarkAttendanceAsync_FutureDate_ShouldThrowBusinessRuleException()
    {
        var dto = new StaffAttendanceRequestDTO(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "present", null, null);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });

        Func<Task> act = async () => await _service.MarkAttendanceAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Attendance date cannot be in the future.");
    }

    [Fact]
    public async Task GetAttendanceByUserAsync_ShouldReturnAttendance()
    {
        var items = new List<Staffattendance>
        {
            new Staffattendance { Id = 1, Userid = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) }
        };
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(items.BuildMockDbSet().Object);

        var result = await _service.GetAttendanceByUserAsync(1, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAttendanceByDateAsync_ShouldReturnAttendance()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = new List<Staffattendance>
        {
            new Staffattendance { Id = 1, Userid = 1, Date = date }
        };
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(items.BuildMockDbSet().Object);

        var result = await _service.GetAllAttendanceByDateAsync(date, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
