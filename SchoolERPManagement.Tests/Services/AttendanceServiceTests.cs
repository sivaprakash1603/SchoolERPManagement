using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class AttendanceServiceTests
{
    private readonly Mock<IRepository<int, Attendance>> _attendanceRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _studentEnrollmentRepoMock;
    private readonly Mock<IRepository<int, Academiccalendar>> _calendarRepoMock;
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AttendanceService _attendanceService;

    public AttendanceServiceTests()
    {
        _attendanceRepoMock = new Mock<IRepository<int, Attendance>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _studentEnrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _calendarRepoMock = new Mock<IRepository<int, Academiccalendar>>();
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();
        _notificationServiceMock = new Mock<INotificationService>();

        _attendanceService = new AttendanceService(
            _attendanceRepoMock.Object,
            _studentRepoMock.Object,
            _teacherRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _calendarRepoMock.Object,
            _academicYearRepoMock.Object,
            _notificationServiceMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task MarkAttendanceAsync_NewAttendance_ShouldCreateAttendance()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "Present", 1, "On time");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        var classEntity = new Class { Id = 1, Classteacherid = 1 };
        var enrollment = new Studentenrollment { Studentid = 1, Class = classEntity };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);

        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance>().BuildMockDbSet().Object);

        
        var result = await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.StudentId.Should().Be(1);
        result.Status.Should().Be("Present");

        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendanceAsync_ExistingAttendance_ShouldUpdateAttendance()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "Absent", null, "Sick");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        var existingAttendance = new Attendance { Id = 1, Studentid = 1, Date = DateOnly.Parse("2025-01-01"), Status = "Present" };
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance> { existingAttendance }.BuildMockDbSet().Object);

        
        var result = await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        result.Status.Should().Be("Absent");
        _attendanceRepoMock.Verify(r => r.UpdateAsync(existingAttendance, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendanceAsync_InvalidStudent_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new MarkAttendanceDTO(999, DateOnly.Parse("2025-01-01"), "Present", null, null);
        _studentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Student?)null);

        
        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Student with identifier '999' was not found.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_InvalidTeacher_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "Present", 999, null);
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Teacher?)null);

        
        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Teacher with identifier '999' was not found.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_NotClassTeacher_ShouldThrowBusinessRuleException()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "Present", 2, null);

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Teacher { Id = 2 });

        var classEntity = new Class { Id = 1, Classteacherid = 1 }; 
        var enrollment = new Studentenrollment { Studentid = 1, Class = classEntity };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);

        
        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only the designated class teacher can mark attendance for this student.");
    }
}
