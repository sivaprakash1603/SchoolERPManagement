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

        _studentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Student> { new Student { Id = 1 } }.BuildMockDbSet().Object);
        _studentEnrollmentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);
        _calendarRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);
        _attendanceRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Attendance>().BuildMockDbSet().Object);
    }

    [Fact]
    public async Task MarkAttendanceAsync_NewAttendance_ShouldCreateAttendance()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "Present", 1, "On time");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        var classEntity = new Class { Id = 1, Classteacherid = 1 };
        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Class = classEntity,
            Academicyear = new Academicyear { Startdate = DateOnly.Parse("2024-01-01"), Enddate = DateOnly.Parse("2025-12-31") }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance>().BuildMockDbSet().Object);

        
        var result = await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.StudentId.Should().Be(1);
        result.Status.Should().Be("present");

        _attendanceRepoMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendanceAsync_ExistingAttendance_ShouldUpdateAttendance()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "Absent", null, "Sick");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        var classEntity = new Class { Id = 1, Classteacherid = 1 };
        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Class = classEntity,
            Academicyear = new Academicyear { Startdate = DateOnly.Parse("2024-01-01"), Enddate = DateOnly.Parse("2025-12-31") }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);

        var existingAttendance = new Attendance { Id = 1, Studentid = 1, Date = DateOnly.Parse("2025-01-01"), Status = "Present" };
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance> { existingAttendance }.BuildMockDbSet().Object);

        
        var result = await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        result.Status.Should().Be("absent");
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

        var classEntity = new Class { Id = 1, Classteacherid = 1 };
        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Class = classEntity,
            Academicyear = new Academicyear { Startdate = DateOnly.Parse("2024-01-01"), Enddate = DateOnly.Parse("2025-12-31") }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);

        
        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Teacher with identifier '999' was not found.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_NotClassTeacher_ShouldThrowBusinessRuleException()
    {
        
        var dto = new MarkAttendanceDTO(1, DateOnly.Parse("2025-01-01"), "present", 2, null);

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Teacher { Id = 2 });

        var classEntity = new Class { Id = 1, Classteacherid = 1 }; 
        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Class = classEntity,
            Academicyear = new Academicyear { Startdate = DateOnly.Parse("2024-01-01"), Enddate = DateOnly.Parse("2025-12-31") }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);

        
        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only the designated class teacher can mark attendance for this student.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_FutureDate_ShouldThrowBusinessRuleException()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var dto = new MarkAttendanceDTO(1, futureDate, "Present", null, "On time");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        Func<Task> act = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Attendance date cannot be in the future.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_Sunday_ShouldThrowBusinessRuleException()
    {
        // Find the next Sunday (or any known Sunday, e.g. 2025-01-05 is a Sunday)
        var sundayDate = DateOnly.Parse("2025-01-05");
        var dto = new MarkAttendanceDTO(1, sundayDate, "Present", null, "On time");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        Func<Task> act = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Cannot mark attendance on a Sunday.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_NoEnrollmentEver_ShouldThrowBusinessRuleException()
    {
        var dto = new MarkAttendanceDTO(1, DateOnly.FromDateTime(DateTime.UtcNow), "present", null, null);
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Student is not enrolled in any class.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_NoEnrollmentForDate_ShouldThrowBusinessRuleException()
    {
        var dto = new MarkAttendanceDTO(1, DateOnly.FromDateTime(DateTime.UtcNow), "present", null, null);
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Academicyear = new Academicyear { Startdate = DateOnly.Parse("2020-01-01"), Enddate = DateOnly.Parse("2020-12-31") }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);

        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("No enrollment found for this student that covers the selected attendance date. Ensure the student is enrolled in a class for the relevant academic year.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_OnHoliday_ShouldThrowBusinessRuleException()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var dto = new MarkAttendanceDTO(1, date, "present", null, null);
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        
        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Academicyear = new Academicyear { Id = 1, Startdate = date.AddDays(-10), Enddate = date.AddDays(10) }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);

        var calendar = new Academiccalendar { Academicyearid = 1, Date = date, Isholiday = true };
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar> { calendar }.BuildMockDbSet().Object);

        Func<Task> action = async () => await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Cannot mark attendance on a school holiday.");
    }

    [Fact]
    public async Task MarkAttendanceAsync_Absent_ShouldSendNotificationToParents()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var dto = new MarkAttendanceDTO(1, date, "absent", null, null);
        
        var student = new Student { Id = 1, Name = "John Doe" };
        var parent = new Parent { Id = 1, Userid = 101 };
        var studentParent = new Studentparent { Studentid = 1, Parentid = 1, Parent = parent };
        student.Studentparents.Add(studentParent);

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);
        _studentRepoMock.Setup(r => r.Query(true)).Returns(new List<Student> { student }.BuildMockDbSet().Object);

        var enrollment = new Studentenrollment { 
            Studentid = 1, 
            Academicyear = new Academicyear { Id = 1, Startdate = date.AddDays(-10), Enddate = date.AddDays(10) }
        };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance>().BuildMockDbSet().Object);

        await _attendanceService.MarkAttendanceAsync(dto, CancellationToken.None);

        _notificationServiceMock.Verify(n => n.SendNotificationAsync(
            It.Is<SchoolERPManagementBLLibrary.DTOs.Notification.SendNotificationDTO>(n => n.Title == "Student Absent Alert" && n.TargetUserIds.Contains(101)), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetAttendanceByStudentAsync_ShouldReturnAttendance()
    {
        var attendance = new Attendance { Id = 1, Studentid = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), Status = "present" };
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance> { attendance }.BuildMockDbSet().Object);

        var result = await _attendanceService.GetAttendanceByStudentAsync(1, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAttendanceByClassAsync_ShouldReturnAttendance()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var enrollment = new Studentenrollment { Classid = 1, Studentid = 1 };
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentenrollment> { enrollment }.BuildMockDbSet().Object);

        var attendance = new Attendance { Id = 1, Studentid = 1, Date = date, Status = "present" };
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(new List<Attendance> { attendance }.BuildMockDbSet().Object);

        var result = await _attendanceService.GetAttendanceByClassAsync(1, date, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
