using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IRepository<int, Parent>> _parentRepoMock;
    private readonly Mock<IRepository<int, Feepayment>> _feePaymentRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IRepository<int, Asset>> _assetRepoMock;
    private readonly Mock<IRepository<int, Attendance>> _attendanceRepoMock;
    private readonly Mock<IRepository<int, Staffattendance>> _staffAttendanceRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _studentEnrollmentRepoMock;
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _parentRepoMock = new Mock<IRepository<int, Parent>>();
        _feePaymentRepoMock = new Mock<IRepository<int, Feepayment>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _assetRepoMock = new Mock<IRepository<int, Asset>>();
        _attendanceRepoMock = new Mock<IRepository<int, Attendance>>();
        _staffAttendanceRepoMock = new Mock<IRepository<int, Staffattendance>>();
        _studentEnrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();

        _dashboardService = new DashboardService(
            _studentRepoMock.Object,
            _teacherRepoMock.Object,
            _parentRepoMock.Object,
            _feePaymentRepoMock.Object,
            _classRepoMock.Object,
            _assetRepoMock.Object,
            _attendanceRepoMock.Object,
            _staffAttendanceRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _academicYearRepoMock.Object
        );
    }

    [Fact]
    public async Task GetAdminDashboardMetricsAsync_ShouldReturnMetrics()
    {
        
        var students = new List<Student>
        {
            new Student 
            { 
                Id = 1, 
                Studentparents = new List<Studentparent> { new Studentparent { Studentid = 1, Parentid = 1 } }
            },
            new Student 
            { 
                Id = 2, 
                Studentparents = new List<Studentparent> { new Studentparent { Studentid = 2, Parentid = 2 } }
            }
        };
        var teachers = new List<Teacher> { new Teacher { Id = 1 } };
        var parents = new List<Parent> { new Parent { Id = 1 }, new Parent { Id = 2 }, new Parent { Id = 3 } };
        var activeYear = new Academicyear { Id = 1, Yearname = "2026-2027", Iscurrent = true };
        var enrollments = new List<Studentenrollment> 
        {
            new Studentenrollment { Studentid = 1, Academicyearid = 1, Student = students[0] },
            new Studentenrollment { Studentid = 2, Academicyearid = 1, Student = students[1] }
        };
        var fees = new List<Feepayment>
        {
            new Feepayment { Amountpaid = 1000m, Feestructure = new Feestructure { Academicyearid = 1 } },
            new Feepayment { Amountpaid = 500m, Feestructure = new Feestructure { Academicyearid = 1 } }
        };
        var classes = new List<Class> 
        { 
            new Class { Id = 1, Academicyearid = 1 }, 
            new Class { Id = 2, Academicyearid = 1 } 
        };
        var assets = new List<Asset> { new Asset(), new Asset(), new Asset() };
        var attendances = new List<Attendance>();
        var staffAttendances = new List<Staffattendance>();

        _studentRepoMock.Setup(r => r.Query(true)).Returns(students.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(teachers.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(true)).Returns(parents.BuildMockDbSet().Object);
        _feePaymentRepoMock.Setup(r => r.Query(true)).Returns(fees.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.BuildMockDbSet().Object);
        _assetRepoMock.Setup(r => r.Query(true)).Returns(assets.BuildMockDbSet().Object);
        _attendanceRepoMock.Setup(r => r.Query(true)).Returns(attendances.BuildMockDbSet().Object);
        _staffAttendanceRepoMock.Setup(r => r.Query(true)).Returns(staffAttendances.BuildMockDbSet().Object);
        _studentEnrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.BuildMockDbSet().Object);
        _academicYearRepoMock.Setup(r => r.Query(true)).Returns(new List<Academicyear> { activeYear }.BuildMockDbSet().Object);

        
        var result = await _dashboardService.GetAdminDashboardMetricsAsync(null, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.TotalStudents.Should().Be(2);
        result.TotalTeachers.Should().Be(1);
        result.TotalParents.Should().Be(2); // Only parents of enrolled students (2 students, parent 1 and 2)
        result.TotalRevenue.Should().Be(1500m);
        result.TotalClasses.Should().Be(2);
        result.TotalAssets.Should().Be(3);
    }
}
