using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Student;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class StudentServiceTests
{
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly Mock<IRepository<int, Parent>> _parentRepoMock;
    private readonly Mock<IRepository<int, Role>> _roleRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _enrollmentRepoMock;
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly StudentService _studentService;

    public StudentServiceTests()
    {
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _userRepoMock = new Mock<IRepository<int, User>>();
        _parentRepoMock = new Mock<IRepository<int, Parent>>();
        _roleRepoMock = new Mock<IRepository<int, Role>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _enrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();
        _emailServiceMock = new Mock<IEmailService>();

        _studentService = new StudentService(
            _studentRepoMock.Object,
            _userRepoMock.Object,
            _parentRepoMock.Object,
            _roleRepoMock.Object,
            _classRepoMock.Object,
            _enrollmentRepoMock.Object,
            _academicYearRepoMock.Object,
            _emailServiceMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task GetAllStudentsAsync_ShouldFilterAndSort()
    {
        var students = new List<Student>
        {
            new Student { Id = 1, Name = "Alice", Regno = "S001", Gender = "Female", User = new User { Isactive = true }, Studentenrollments = { new Studentenrollment { Classid = 1, Academicyearid = 1 } } },
            new Student { Id = 2, Name = "Bob", Regno = "S002", Gender = "Male", User = new User { Isactive = false }, Studentenrollments = { new Studentenrollment { Classid = 2, Academicyearid = 1 } } }
        };
        _studentRepoMock.Setup(r => r.Query(true)).Returns(students.BuildMockDbSet().Object);
        _academicYearRepoMock.Setup(r => r.Query(false)).Returns(new List<Academicyear> { new Academicyear { Id = 1, Iscurrent = true } }.BuildMockDbSet().Object);

        var request = new StudentQueryRequest { SearchQuery = "alice", Gender = "Female", Status = "Active", SortBy = "name", SortDirection = "desc", PageNumber = 1, PageSize = 10, ClassId = 1 };
        
        var result = await _studentService.GetAllStudentsAsync(request, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetStudentByIdAsync_ValidId_ShouldReturnStudent()
    {
        var student = new Student { Id = 1, Name = "Alice" };
        _studentRepoMock.Setup(r => r.Query(true)).Returns(new List<Student> { student }.BuildMockDbSet().Object);

        var result = await _studentService.GetStudentByIdAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetStudentByIdAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _studentRepoMock.Setup(r => r.Query(true)).Returns(new List<Student>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _studentService.GetStudentByIdAsync(999, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddStudentAsync_ValidData_ShouldAddStudentAndEnrollment()
    {
        var dto = new CreateStudentDTO("jdoe@example.com", "John Doe", 1, 1, new List<ParentSelectionDTO> { new ParentSelectionDTO(1, "Father") });

        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 3, Rolename = "Student" } }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Parent { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });
        _studentRepoMock.Setup(r => r.Query(false)).Returns(new List<Student>().BuildMockDbSet().Object);
        _enrollmentRepoMock.Setup(r => r.Query(false)).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);

        var result = await _studentService.AddStudentAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("John Doe");
        _studentRepoMock.Verify(r => r.AddAsync(It.IsAny<Student>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Studentenrollment>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddStudentAsync_EmailFails_ShouldStillSucceed()
    {
        var dto = new CreateStudentDTO("jdoe@example.com", "John Doe", 1, 1, new List<ParentSelectionDTO>());
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 3, Rolename = "Student" } }.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });
        _studentRepoMock.Setup(r => r.Query(false)).Returns(new List<Student>().BuildMockDbSet().Object);
        _enrollmentRepoMock.Setup(r => r.Query(false)).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);
        
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SMTP Error"));

        var result = await _studentService.AddStudentAsync(dto, CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddStudentAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
    {
        var dto = new CreateStudentDTO("jdoe@example.com", "John Doe", 1, 1, null);
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User> { new User { Email = "jdoe@example.com" } }.BuildMockDbSet().Object);

        Func<Task> action = async () => await _studentService.AddStudentAsync(dto, CancellationToken.None);
        await action.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task UpdateStudentAsync_ValidData_ShouldUpdate()
    {
        var student = new Student { Id = 1, Name = "Old Name" };
        _studentRepoMock.Setup(r => r.Query(false)).Returns(new List<Student> { student }.BuildMockDbSet().Object);

        var dto = new UpdateStudentDTO("New Name", "Male", null, "O+", null, null);
        var result = await _studentService.UpdateStudentAsync(1, dto, CancellationToken.None);

        result.Name.Should().Be("New Name");
        _studentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Student>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateStudentAsync_WithParents_ShouldUpdateParents()
    {
        var student = new Student { Id = 1, Name = "Old Name", Studentparents = new List<Studentparent>() };
        _studentRepoMock.Setup(r => r.Query(false)).Returns(new List<Student> { student }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Parent { Id = 1 });

        var dto = new UpdateStudentDTO("New Name", "Male", new List<ParentSelectionDTO> { new ParentSelectionDTO(1, "Father") }, "O+", null, null);
        var result = await _studentService.UpdateStudentAsync(1, dto, CancellationToken.None);

        _studentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Student>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ValidId_ShouldDeactivateUser()
    {
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1, Userid = 10 });
        var user = new User { Id = 10, Isactive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

        await _studentService.DeleteStudentAsync(1, CancellationToken.None);

        user.Isactive.Should().BeFalse();
        _userRepoMock.Verify(r => r.UpdateAsync(user, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStudentIdByUserIdAsync_ShouldReturnId()
    {
        _studentRepoMock.Setup(r => r.Query(true)).Returns(new List<Student> { new Student { Id = 5, Userid = 10 } }.BuildMockDbSet().Object);
        var result = await _studentService.GetStudentIdByUserIdAsync(10, CancellationToken.None);
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetStudentsByClassIdAsync_ShouldReturnStudents()
    {
        var enrollments = new List<Studentenrollment> { new Studentenrollment { Classid = 1, Student = new Student { Name = "Alice" } } };
        _enrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.BuildMockDbSet().Object);

        var result = await _studentService.GetStudentsByClassIdAsync(1, CancellationToken.None);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task EnrollStudentAsync_ValidData_ShouldEnroll()
    {
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });
        _enrollmentRepoMock.Setup(r => r.Query(false)).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);

        var dto = new EnrollStudentDTO(1, 1);
        await _studentService.EnrollStudentAsync(1, dto, CancellationToken.None);

        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Studentenrollment>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkEnrollStudentsAsync_ValidData_ShouldEnrollMultiple()
    {
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });
        _enrollmentRepoMock.Setup(r => r.Query(false)).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);

        var dto = new BulkEnrollStudentsDTO(new List<int> { 1, 2 }, 1, 1);
        await _studentService.BulkEnrollStudentsAsync(dto, CancellationToken.None);

        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Studentenrollment>(), false, It.IsAny<CancellationToken>()), Times.Once);
        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Studentenrollment>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStudentStatsAsync_ShouldReturnStats()
    {
        var students = new List<Student> { new Student { Id = 1, Userid = 10, Gender = "Male" }, new Student { Id = 2, Userid = 20, Gender = "Female" } };
        _studentRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(students);

        var users = new List<User> { new User { Id = 10, Isactive = true }, new User { Id = 20, Isactive = false } };
        _userRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var result = await _studentService.GetStudentStatsAsync(CancellationToken.None);

        result.TotalStudents.Should().Be(2);
        result.ActiveStudents.Should().Be(1);
        result.InactiveStudents.Should().Be(1);
        result.Boys.Should().Be(1);
        result.Girls.Should().Be(1);
    }
}
