using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Timetable;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class TimetableServiceTests
{
    private readonly Mock<IRepository<int, Timetable>> _timetableRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IRepository<int, Subject>> _subjectRepoMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly TimetableService _timetableService;

    public TimetableServiceTests()
    {
        _timetableRepoMock = new Mock<IRepository<int, Timetable>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();

        _timetableService = new TimetableService(
            _timetableRepoMock.Object,
            _classRepoMock.Object,
            _subjectRepoMock.Object,
            _teacherRepoMock.Object
        ,
            new Moq.Mock<AutoMapper.IMapper>().Object
        );
    }

    [Fact]
    public async Task CreateTimetableAsync_ValidData_ShouldCreateTimetable()
    {
        
        var dto = new CreateTimetableDTO(1, 1, 1, "Monday", TimeOnly.Parse("09:00"), TimeOnly.Parse("10:00"), "Room101");

        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        _timetableRepoMock.Setup(r => r.Query(true)).Returns(new List<Timetable>().AsQueryable().BuildMock());

        
        var result = await _timetableService.CreateTimetableAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.ClassId.Should().Be(1);
        result.SubjectId.Should().Be(1);
        result.TeacherId.Should().Be(1);

        _timetableRepoMock.Verify(r => r.AddAsync(It.IsAny<Timetable>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTimetableAsync_ClassClash_ShouldThrowBusinessRuleException()
    {
        
        var dto = new CreateTimetableDTO(1, 1, 1, "Monday", TimeOnly.Parse("09:00"), TimeOnly.Parse("10:00"), "Room101");

        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        var existingClassTimetable = new Timetable
        {
            Classid = 1, Teacherid = 999, Dayofweek = "Monday", Starttime = TimeOnly.Parse("09:30"), Endtime = TimeOnly.Parse("10:30")
        };
        _timetableRepoMock.Setup(r => r.Query(true)).Returns(new List<Timetable> { existingClassTimetable }.AsQueryable().BuildMock());

        
        Func<Task> action = async () => await _timetableService.CreateTimetableAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("The class timetable overlaps with an existing slot.");
    }

    [Fact]
    public async Task CreateTimetableAsync_TeacherClash_ShouldThrowBusinessRuleException()
    {
        
        var dto = new CreateTimetableDTO(1, 1, 1, "Monday", TimeOnly.Parse("09:00"), TimeOnly.Parse("10:00"), "Room101");

        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        var existingTeacherTimetable = new Timetable
        {
            Classid = 999, Teacherid = 1, Dayofweek = "Monday", Starttime = TimeOnly.Parse("08:30"), Endtime = TimeOnly.Parse("09:30")
        };
        _timetableRepoMock.Setup(r => r.Query(true)).Returns(new List<Timetable> { existingTeacherTimetable }.AsQueryable().BuildMock());

        
        Func<Task> action = async () => await _timetableService.CreateTimetableAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("The teacher already has a timetable slot at this time.");
    }

    [Fact]
    public async Task GetClassTimetableAsync_ShouldReturnTimetable()
    {
        
        var timetables = new List<Timetable>
        {
            new Timetable { Id = 1, Classid = 1, Dayofweek = "Monday", Starttime = TimeOnly.Parse("09:00") },
            new Timetable { Id = 2, Classid = 1, Dayofweek = "Tuesday", Starttime = TimeOnly.Parse("09:00") }
        };

        _timetableRepoMock.Setup(r => r.Query(true)).Returns(timetables.AsQueryable().BuildMock());

        
        var result = await _timetableService.GetClassTimetableAsync(1, CancellationToken.None);

        
        result.Should().HaveCount(2);
        result.First().DayOfWeek.Should().Be("Monday");
    }
}
