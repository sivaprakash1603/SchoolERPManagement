using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Teacher;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class TeacherServiceTests
{
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly Mock<IRepository<int, Subject>> _subjectRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IRepository<int, Teachersubject>> _teacherSubjectRepoMock;
    private readonly Mock<IRepository<int, Timetable>> _timetableRepoMock;
    private readonly Mock<IRepository<int, Role>> _roleRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly TeacherService _teacherService;

    public TeacherServiceTests()
    {
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _userRepoMock = new Mock<IRepository<int, User>>();
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _teacherSubjectRepoMock = new Mock<IRepository<int, Teachersubject>>();
        _timetableRepoMock = new Mock<IRepository<int, Timetable>>();
        _roleRepoMock = new Mock<IRepository<int, Role>>();
        _emailServiceMock = new Mock<IEmailService>();

        _teacherService = new TeacherService(
            _teacherRepoMock.Object,
            _userRepoMock.Object,
            _subjectRepoMock.Object,
            _classRepoMock.Object,
            _teacherSubjectRepoMock.Object,
            _timetableRepoMock.Object,
            _roleRepoMock.Object,
            _emailServiceMock.Object
        );
    }

    [Fact]
    public async Task GetAllTeachersAsync_ShouldFilterAndSort()
    {
        var teachers = new List<Teacher>
        {
            new Teacher { Id = 1, Name = "Alice", User = new User { Isactive = true } },
            new Teacher { Id = 2, Name = "Bob", User = new User { Isactive = false } }
        };
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(teachers.BuildMockDbSet().Object);

        var request = new TeacherQueryRequest { SearchQuery = "alice", Status = "Active", SortBy = "name", SortDirection = "desc", PageNumber = 1, PageSize = 10 };
        
        var result = await _teacherService.GetAllTeachersAsync(request, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetTeacherByIdAsync_ValidId_ShouldReturnTeacher()
    {
        var teacher = new Teacher { Id = 1, Name = "Alice", User = new User { Username = "alice" } };
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(new List<Teacher> { teacher }.BuildMockDbSet().Object);

        var result = await _teacherService.GetTeacherByIdAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetTeacherByIdAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(new List<Teacher>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _teacherService.GetTeacherByIdAsync(999, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetTeacherByUsernameAsync_ValidUsername_ShouldReturnTeacher()
    {
        var teacher = new Teacher { Id = 1, Name = "Alice", User = new User { Username = "alice" } };
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(new List<Teacher> { teacher }.BuildMockDbSet().Object);

        var result = await _teacherService.GetTeacherByUsernameAsync("alice", CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetTeacherByUsernameAsync_InvalidUsername_ShouldThrowEntityNotFoundException()
    {
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(new List<Teacher>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _teacherService.GetTeacherByUsernameAsync("unknown", CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddTeacherAsync_ValidData_ShouldCreateTeacherAndUser()
    {
        var dto = new CreateTeacherDTO("jane@example.com", "Jane Smith", "9876543210", "Masters");
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 2, Rolename = "Teacher" } }.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.Query(false)).Returns(new List<Teacher>().BuildMockDbSet().Object);

        var result = await _teacherService.AddTeacherAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Jane Smith");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _teacherRepoMock.Verify(r => r.AddAsync(It.IsAny<Teacher>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddTeacherAsync_EmailFails_ShouldStillSucceed()
    {
        var dto = new CreateTeacherDTO("jane@example.com", "Jane Smith", "9876543210", "Masters");
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 2, Rolename = "Teacher" } }.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.Query(false)).Returns(new List<Teacher>().BuildMockDbSet().Object);
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SMTP Error"));

        var result = await _teacherService.AddTeacherAsync(dto, CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddTeacherAsync_RoleNotFound_ShouldThrowEntityNotFoundException()
    {
        var dto = new CreateTeacherDTO("jane@example.com", "Jane Smith", "9876543210", "Masters");
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _teacherService.AddTeacherAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddTeacherAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
    {
        var dto = new CreateTeacherDTO("jane@example.com", "Jane Smith", "9876543210", "Masters");
        var existingUser = new User { Email = "jane@example.com" };
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User> { existingUser }.BuildMockDbSet().Object);

        Func<Task> action = async () => await _teacherService.AddTeacherAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task AssignSubjectAsync_ValidData_ShouldAssign()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _teacherSubjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Teachersubject>().BuildMockDbSet().Object);
        _teacherSubjectRepoMock.Setup(r => r.Query(false)).Returns(new List<Teachersubject>().BuildMockDbSet().Object);

        var dto = new AssignTeacherSubjectDTO(1, 1, 1);
        var result = await _teacherService.AssignSubjectAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        _teacherSubjectRepoMock.Verify(r => r.AddAsync(It.IsAny<Teachersubject>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AssignSubjectAsync_AlreadyAssignedToOther_ShouldThrowBusinessRuleException()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _teacherSubjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Teachersubject>().BuildMockDbSet().Object);
        _teacherSubjectRepoMock.Setup(r => r.Query(false)).Returns(new List<Teachersubject> { new Teachersubject { Teacherid = 2, Subjectid = 1, Classid = 1, Teacher = new Teacher { Name = "Other" } } }.BuildMockDbSet().Object);

        var dto = new AssignTeacherSubjectDTO(1, 1, 1);
        Func<Task> action = async () => await _teacherService.AssignSubjectAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task AssignSubjectAsync_InvalidTeacher_ShouldThrowEntityNotFoundException()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Teacher?)null);
        var dto = new AssignTeacherSubjectDTO(1, 1, 1);
        Func<Task> action = async () => await _teacherService.AssignSubjectAsync(dto, CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetTeacherAssignmentsAsync_ShouldReturnAssignments()
    {
        var assignments = new List<Teachersubject> { new Teachersubject { Teacherid = 1, Class = new Class { Classname = "10" }, Subject = new Subject { Subjectname = "Math" } } };
        _teacherSubjectRepoMock.Setup(r => r.Query(false)).Returns(assignments.BuildMockDbSet().Object);

        var result = await _teacherService.GetTeacherAssignmentsAsync(1, CancellationToken.None);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteTeacherAssignmentAsync_ValidAssignment_ShouldDelete()
    {
        var assignment = new Teachersubject { Teacherid = 1, Classid = 1, Subjectid = 1 };
        _teacherSubjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Teachersubject> { assignment }.BuildMockDbSet().Object);

        await _teacherService.DeleteTeacherAssignmentAsync(1, 1, 1, CancellationToken.None);

        _teacherSubjectRepoMock.Verify(r => r.DeleteAsync(assignment, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyTeacherAssignmentAsync_ShouldReturnTrueIfAssigned()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1, Userid = 10 });
        _teacherSubjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Teachersubject> { new Teachersubject { Teacherid = 1, Classid = 1, Subjectid = 1 } }.BuildMockDbSet().Object);
        _timetableRepoMock.Setup(r => r.Query(true)).Returns(new List<Timetable>().BuildMockDbSet().Object);

        var result = await _teacherService.VerifyTeacherAssignmentAsync(10, 1, 1, 1, CancellationToken.None);
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task VerifyTeacherAssignmentAsync_InvalidUser_ShouldReturnFalse()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Teacher?)null);
        var result = await _teacherService.VerifyTeacherAssignmentAsync(10, 1, 1, 1, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTeacherAsync_ValidData_ShouldUpdate()
    {
        var teacher = new Teacher { Id = 1, Name = "Old Name" };
        _teacherRepoMock.Setup(r => r.Query(false)).Returns(new List<Teacher> { teacher }.BuildMockDbSet().Object);
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(new List<Teacher> { teacher }.BuildMockDbSet().Object);

        var dto = new UpdateTeacherDTO("New Name", "999", "Phd", 1);
        var result = await _teacherService.UpdateTeacherAsync(1, dto, CancellationToken.None);

        result.Name.Should().Be("New Name");
        _teacherRepoMock.Verify(r => r.UpdateAsync(teacher, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeacherAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _teacherRepoMock.Setup(r => r.Query(false)).Returns(new List<Teacher>().BuildMockDbSet().Object);
        var dto = new UpdateTeacherDTO("New Name", "999", "Phd");
        Func<Task> action = async () => await _teacherService.UpdateTeacherAsync(999, dto, CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteTeacherAsync_ValidId_ShouldDeactivateUser()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1, Userid = 10 });
        var user = new User { Id = 10, Isactive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

        await _teacherService.DeleteTeacherAsync(1, CancellationToken.None);

        user.Isactive.Should().BeFalse();
        _userRepoMock.Verify(r => r.UpdateAsync(user, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTeacherAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _teacherRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Teacher?)null);
        Func<Task> action = async () => await _teacherService.DeleteTeacherAsync(999, CancellationToken.None);
        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetTeacherStatsAsync_ShouldReturnStats()
    {
        var teachers = new List<Teacher> { new Teacher { Id = 1, Userid = 10 }, new Teacher { Id = 2, Userid = 20 } };
        _teacherRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(teachers);

        var users = new List<User> { new User { Id = 10, Isactive = true }, new User { Id = 20, Isactive = false } };
        _userRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var result = await _teacherService.GetTeacherStatsAsync(CancellationToken.None);

        result.TotalTeachers.Should().Be(2);
        result.ActiveTeachers.Should().Be(1);
        result.InactiveTeachers.Should().Be(1);
    }

    [Fact]
    public async Task AutoAssignTeachersAsync_ShouldAssign()
    {
        var cls = new Class { Id = 1, Classname = "10", Classsubjects = { new Classsubject { Subjectid = 1 } }, Teachersubjects = new List<Teachersubject>() };
        _classRepoMock.Setup(r => r.Query(false)).Returns(new List<Class> { cls }.BuildMockDbSet().Object);

        var teacher = new Teacher { Id = 1, SubjectSpecialtyId = 1, Teachersubjects = new List<Teachersubject>() };
        _teacherRepoMock.Setup(r => r.Query(false)).Returns(new List<Teacher> { teacher }.BuildMockDbSet().Object);

        var result = await _teacherService.AutoAssignTeachersAsync(CancellationToken.None);

        result.TotalAssignmentsMade.Should().Be(1);
        _classRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
