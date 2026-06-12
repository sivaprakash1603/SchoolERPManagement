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
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _parentRepoMock = new Mock<IRepository<int, Parent>>();
        _feePaymentRepoMock = new Mock<IRepository<int, Feepayment>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _assetRepoMock = new Mock<IRepository<int, Asset>>();

        _dashboardService = new DashboardService(
            _studentRepoMock.Object,
            _teacherRepoMock.Object,
            _parentRepoMock.Object,
            _feePaymentRepoMock.Object,
            _classRepoMock.Object,
            _assetRepoMock.Object
        );
    }

    [Fact]
    public async Task GetAdminDashboardMetricsAsync_ShouldReturnMetrics()
    {
        
        var students = new List<Student> { new Student(), new Student() };
        var teachers = new List<Teacher> { new Teacher() };
        var parents = new List<Parent> { new Parent(), new Parent(), new Parent() };
        var fees = new List<Feepayment>
        {
            new Feepayment { Amountpaid = 1000m },
            new Feepayment { Amountpaid = 500m }
        };
        var classes = new List<Class> { new Class(), new Class() };
        var assets = new List<Asset> { new Asset(), new Asset(), new Asset() };

        _studentRepoMock.Setup(r => r.Query(true)).Returns(students.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(teachers.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(true)).Returns(parents.BuildMockDbSet().Object);
        _feePaymentRepoMock.Setup(r => r.Query(true)).Returns(fees.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.BuildMockDbSet().Object);
        _assetRepoMock.Setup(r => r.Query(true)).Returns(assets.BuildMockDbSet().Object);

        
        var result = await _dashboardService.GetAdminDashboardMetricsAsync(CancellationToken.None);

        
        result.Should().NotBeNull();
        result.TotalStudents.Should().Be(2);
        result.TotalTeachers.Should().Be(1);
        result.TotalParents.Should().Be(3);
        result.TotalRevenue.Should().Be(1500m);
        result.TotalClasses.Should().Be(2);
        result.TotalAssets.Should().Be(3);
    }
}
