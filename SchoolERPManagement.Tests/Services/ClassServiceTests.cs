using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Class;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class ClassServiceTests
{
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _studentEnrollmentRepoMock;
    private readonly Mock<IRepository<int, Teachersubject>> _teacherSubjectRepoMock;
    private readonly Mock<IRepository<int, Timetable>> _timetableRepoMock;
    private readonly Mock<IRepository<int, Homework>> _homeworkRepoMock;
    private readonly Mock<IRepository<int, Feestructure>> _feeStructureRepoMock;
    private readonly Mock<IRepository<int, Examschedule>> _examScheduleRepoMock;
    private readonly Mock<IRepository<int, Asset>> _assetRepoMock;
    private readonly ClassService _classService;

    public ClassServiceTests()
    {
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();
        _studentEnrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _teacherSubjectRepoMock = new Mock<IRepository<int, Teachersubject>>();
        _timetableRepoMock = new Mock<IRepository<int, Timetable>>();
        _homeworkRepoMock = new Mock<IRepository<int, Homework>>();
        _feeStructureRepoMock = new Mock<IRepository<int, Feestructure>>();
        _examScheduleRepoMock = new Mock<IRepository<int, Examschedule>>();
        _assetRepoMock = new Mock<IRepository<int, Asset>>();

        _classService = new ClassService(
            _classRepoMock.Object, 
            _teacherRepoMock.Object, 
            _academicYearRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _teacherSubjectRepoMock.Object,
            _timetableRepoMock.Object,
            _homeworkRepoMock.Object,
            _feeStructureRepoMock.Object,
            _examScheduleRepoMock.Object,
            _assetRepoMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task GetAllClassesAsync_ShouldReturnListOfClasses()
    {
        
        var classes = new List<Class>
        {
            new Class { Id = 1, Classname = "10", Section = "A" },
            new Class { Id = 2, Classname = "10", Section = "B" }
        };

        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.BuildMockDbSet().Object);

        
        var result = await _classService.GetAllClassesAsync(null, CancellationToken.None);

        
        result.Should().HaveCount(2);
        result.First().Classname.Should().Be("10");
        result.First().Section.Should().Be("A");
    }

    [Fact]
    public async Task CreateClassAsync_ValidData_ShouldCreateClass()
    {
        
        var dto = new CreateClassDTO("10", "A", 1, null);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        
        var result = await _classService.CreateClassAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Classname.Should().Be("10");
        result.Section.Should().Be("A");
        
        _classRepoMock.Verify(r => r.AddAsync(It.IsAny<Class>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClassAsync_InvalidTeacher_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new CreateClassDTO("10", "A", 999, null);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Teacher?)null);

        
        Func<Task> action = async () => await _classService.CreateClassAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Teacher with identifier '999' was not found.");
    }
}
