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
            _emailServiceMock.Object
        ,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task AddStudentAsync_ValidData_ShouldAddStudentAndEnrollment()
    {
        
        var dto = new CreateStudentDTO("jdoe@example.com", "John Doe", 1, 1, 1);

        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 3, Rolename = "Student" } }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Parent { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1, Classname = "10", Section = "A" });
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });
        _studentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Student>().BuildMockDbSet().Object);
        _enrollmentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);

        
        var result = await _studentService.AddStudentAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Name.Should().Be("John Doe");
        
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _studentRepoMock.Verify(r => r.AddAsync(It.IsAny<Student>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Studentenrollment>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddStudentAsync_MissingParent_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new CreateStudentDTO("jdoe@example.com", "John Doe", 1, 1, 1);

        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 3, Rolename = "Student" } }.BuildMockDbSet().Object);
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1, Classname = "10", Section = "A" });
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Parent?)null);
        _studentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Student>().BuildMockDbSet().Object);
        _enrollmentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Studentenrollment>().BuildMockDbSet().Object);

        
        Func<Task> action = async () => await _studentService.AddStudentAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Parent with identifier '1' was not found.");
    }
}
