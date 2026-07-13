using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly Mock<IRepository<int, Classsubject>> _classSubjectRepoMock;
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
        _classSubjectRepoMock = new Mock<IRepository<int, Classsubject>>();

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
            _classSubjectRepoMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );

        _academicYearRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Academicyear> { new Academicyear { Id = 1, Iscurrent = true } }.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Class>().BuildMockDbSet().Object);
    }

    [Fact]
    public async Task GetAllClassesAsync_ShouldReturnListOfClasses()
    {
        var classes = new List<Class>
        {
            new Class { Id = 1, Classname = "10", Section = "A", Academicyearid = 1 },
            new Class { Id = 2, Classname = "10", Section = "B", Academicyearid = 1 }
        };

        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.Query(false)).Returns(classes.BuildMockDbSet().Object);

        var result = await _classService.GetAllClassesAsync(null, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Classname.Should().Be("10");
        result.First().Section.Should().Be("A");
    }

    [Fact]
    public async Task GetAllClassesAsync_NoCurrentYear_ShouldStillWork()
    {
        _academicYearRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Academicyear>().BuildMockDbSet().Object);
        var classes = new List<Class> { new Class { Id = 1, Classname = "10", Section = "A" } };
        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.BuildMockDbSet().Object);

        var result = await _classService.GetAllClassesAsync(null, CancellationToken.None);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllClassesAsync_WithSpecificYear_ShouldFilter()
    {
        var classes = new List<Class>
        {
            new Class { Id = 1, Classname = "10", Section = "A", Academicyearid = 2 },
            new Class { Id = 2, Classname = "10", Section = "B", Academicyearid = 2 }
        };
        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.BuildMockDbSet().Object);

        var result = await _classService.GetAllClassesAsync(2, CancellationToken.None);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateClassAsync_ValidData_ShouldCreateClass()
    {
        var dto = new CreateClassDTO("10", "A", 1, null, new List<int> { 101, 102 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        var createdClass = new Class { Id = 5, Classname = "10", Section = "A", Classteacherid = 1, Academicyearid = 1 };
        
        _classRepoMock.Setup(r => r.AddAsync(It.IsAny<Class>(), true, It.IsAny<CancellationToken>()))
            .Callback<Class, bool, CancellationToken>((c, s, t) => c.Id = 5) // simulate DB id generation
            .Returns(Task.CompletedTask);

        _classRepoMock.Setup(r => r.Query(true)).Returns(new List<Class> { createdClass }.BuildMockDbSet().Object);

        var result = await _classService.CreateClassAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Classname.Should().Be("10");
        result.Section.Should().Be("A");
        
        _classRepoMock.Verify(r => r.AddAsync(It.IsAny<Class>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _classSubjectRepoMock.Verify(r => r.AddAsync(It.IsAny<Classsubject>(), false, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _classSubjectRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClassAsync_DuplicateClass_ShouldThrowBusinessRuleException()
    {
        var existing = new Class { Classname = "10", Section = "A", Academicyearid = 1 };
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { existing }.BuildMockDbSet().Object);
        var dto = new CreateClassDTO("10", "a", null, null, null);

        Func<Task> action = async () => await _classService.CreateClassAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("A class with the same name and section already exists in this academic year.");
    }

    [Fact]
    public async Task CreateClassAsync_InvalidTeacher_ShouldThrowEntityNotFoundException()
    {
        var dto = new CreateClassDTO("10", "A", 999, null, null);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Teacher?)null);

        Func<Task> action = async () => await _classService.CreateClassAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateClassAsync_TeacherAlreadyAssigned_ShouldThrowBusinessRuleException()
    {
        var existing = new Class { Id = 99, Classteacherid = 1, Academicyearid = 1 };
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { existing }.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        var dto = new CreateClassDTO("10", "A", 1, null, null);

        Func<Task> action = async () => await _classService.CreateClassAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("A teacher cannot be the class teacher for more than one class in the same academic year.");
    }

    [Fact]
    public async Task UpdateClassAsync_ValidData_ShouldUpdateClass()
    {
        var existingClass = new Class { Id = 1, Classname = "9", Section = "A", Academicyearid = 1 };
        var classSubjects = new List<Classsubject> { new Classsubject { Classid = 1, Subjectid = 101 } };
        
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { existingClass }.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.Query(true)).Returns(new List<Class> { existingClass }.BuildMockDbSet().Object);
        _classSubjectRepoMock.Setup(r => r.Query(false)).Returns(classSubjects.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Teacher { Id = 2 });

        var dto = new UpdateClassDTO("10", "B", 2, null, new List<int> { 102 });

        var result = await _classService.UpdateClassAsync(1, dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Classname.Should().Be("10");
        result.Section.Should().Be("B");

        _classRepoMock.Verify(r => r.UpdateAsync(existingClass, true, It.IsAny<CancellationToken>()), Times.Once);
        _classSubjectRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Classsubject>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _classSubjectRepoMock.Verify(r => r.AddAsync(It.IsAny<Classsubject>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _classSubjectRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateClassAsync_ClassNotFound_ShouldThrowEntityNotFoundException()
    {
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class>().BuildMockDbSet().Object);
        var dto = new UpdateClassDTO("10", "B", null, null, null);

        Func<Task> action = async () => await _classService.UpdateClassAsync(999, dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateClassAsync_DuplicateClass_ShouldThrowBusinessRuleException()
    {
        var classToUpdate = new Class { Id = 1, Classname = "9", Section = "A", Academicyearid = 1 };
        var existingConflict = new Class { Id = 2, Classname = "10", Section = "A", Academicyearid = 1 };
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { classToUpdate, existingConflict }.BuildMockDbSet().Object);
        
        var dto = new UpdateClassDTO("10", "A", null, null, null);

        Func<Task> action = async () => await _classService.UpdateClassAsync(1, dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("A class with the same name and section already exists in this academic year.");
    }

    [Fact]
    public async Task UpdateClassAsync_TeacherNotFound_ShouldThrowEntityNotFoundException()
    {
        var classToUpdate = new Class { Id = 1, Classname = "9", Section = "A", Academicyearid = 1 };
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { classToUpdate }.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Teacher?)null);

        var dto = new UpdateClassDTO("9", "A", 2, null, null);

        Func<Task> action = async () => await _classService.UpdateClassAsync(1, dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateClassAsync_TeacherAlreadyAssigned_ShouldThrowBusinessRuleException()
    {
        var classToUpdate = new Class { Id = 1, Classname = "9", Section = "A", Academicyearid = 1, Classteacherid = 1 };
        var existingConflict = new Class { Id = 2, Classname = "10", Section = "A", Academicyearid = 1, Classteacherid = 2 };
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { classToUpdate, existingConflict }.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Teacher { Id = 2 });

        var dto = new UpdateClassDTO("9", "A", 2, null, null);

        Func<Task> action = async () => await _classService.UpdateClassAsync(1, dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("A teacher cannot be the class teacher for more than one class in the same academic year.");
    }

    [Fact]
    public async Task DeleteClassAsync_ValidId_ShouldCascadeDeleteAndNullifyAssets()
    {
        var classEntity = new Class
        {
            Id = 1,
            Studentenrollments = new List<Studentenrollment> { new Studentenrollment { Id = 1 } },
            Classsubjects = new List<Classsubject> { new Classsubject { Id = 1 } },
            Teachersubjects = new List<Teachersubject> { new Teachersubject { Id = 1 } },
            Timetables = new List<Timetable> { new Timetable { Id = 1 } },
            Homeworks = new List<Homework> { new Homework { Id = 1 } },
            Feestructures = new List<Feestructure> { new Feestructure { Id = 1 } },
            Examschedules = new List<Examschedule> { new Examschedule { Id = 1 } },
            Assets = new List<Asset> { new Asset { Id = 1, Assignedclassid = 1 } }
        };

        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { classEntity }.BuildMockDbSet().Object);

        await _classService.DeleteClassAsync(1, CancellationToken.None);

        _studentEnrollmentRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Studentenrollment>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _classSubjectRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Classsubject>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _teacherSubjectRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Teachersubject>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _timetableRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Timetable>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _homeworkRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Homework>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _feeStructureRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Feestructure>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _examScheduleRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Examschedule>(), false, It.IsAny<CancellationToken>()), Times.Once);
        
        _assetRepoMock.Verify(r => r.UpdateAsync(It.Is<Asset>(a => a.Assignedclassid == null), false, It.IsAny<CancellationToken>()), Times.Once);
        
        _classRepoMock.Verify(r => r.DeleteAsync(classEntity, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClassAsync_ClassNotFound_ShouldThrowEntityNotFoundException()
    {
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _classService.DeleteClassAsync(999, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }
}
