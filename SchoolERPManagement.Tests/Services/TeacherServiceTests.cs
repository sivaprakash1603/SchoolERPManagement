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
    private readonly Mock<IRepository<int, Role>> _roleRepoMock;
    private readonly Mock<IRepository<int, SchoolERPManagementModelLibrary.Models.Salary>> _salaryRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly TeacherService _teacherService;

    public TeacherServiceTests()
    {
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _userRepoMock = new Mock<IRepository<int, User>>();
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _teacherSubjectRepoMock = new Mock<IRepository<int, Teachersubject>>();
        _roleRepoMock = new Mock<IRepository<int, Role>>();
        _salaryRepoMock = new Mock<IRepository<int, SchoolERPManagementModelLibrary.Models.Salary>>();
        _emailServiceMock = new Mock<IEmailService>();

        _teacherService = new TeacherService(
            _teacherRepoMock.Object,
            _userRepoMock.Object,
            _subjectRepoMock.Object,
            _classRepoMock.Object,
            _teacherSubjectRepoMock.Object,
            _roleRepoMock.Object,
            _salaryRepoMock.Object,
            _emailServiceMock.Object
        ,
            new Moq.Mock<AutoMapper.IMapper>().Object
        );
    }

    [Fact]
    public async Task AddTeacherAsync_ValidData_ShouldCreateTeacherAndUser()
    {
        
        var dto = new CreateTeacherDTO("jane@example.com", "Jane Smith", "9876543210", "Masters", 50000m);

        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().AsQueryable().BuildMock());
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 2, Rolename = "Teacher" } }.AsQueryable().BuildMock());
        _teacherRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Teacher>().AsQueryable().BuildMock());

        
        var result = await _teacherService.AddTeacherAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Name.Should().Be("Jane Smith");

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _teacherRepoMock.Verify(r => r.AddAsync(It.IsAny<Teacher>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTeacherAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
    {
        
        var dto = new CreateTeacherDTO("jane@example.com", "Jane Smith", "9876543210", "Masters", 50000m);

        var existingUser = new User { Email = "jane@example.com" };
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User> { existingUser }.AsQueryable().BuildMock());

        
        Func<Task> action = async () => await _teacherService.AddTeacherAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<DuplicateEntityException>().WithMessage("User with Email 'jane@example.com' already exists.");
    }
}
