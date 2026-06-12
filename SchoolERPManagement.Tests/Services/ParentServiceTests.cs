using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Parent;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class ParentServiceTests
{
    private readonly Mock<IRepository<int, Parent>> _parentRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _enrollmentRepoMock;
    private readonly Mock<IRepository<int, User>> _userRepoMock;
    private readonly Mock<IRepository<int, Role>> _roleRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ParentService _parentService;

    public ParentServiceTests()
    {
        _parentRepoMock = new Mock<IRepository<int, Parent>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _enrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _userRepoMock = new Mock<IRepository<int, User>>();
        _roleRepoMock = new Mock<IRepository<int, Role>>();
        _emailServiceMock = new Mock<IEmailService>();

        _parentService = new ParentService(
            _parentRepoMock.Object,
            _studentRepoMock.Object,
            _enrollmentRepoMock.Object,
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _emailServiceMock.Object
        ,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task AddParentAsync_ValidData_ShouldCreateParentAndUser()
    {
        
        var dto = new CreateParentDTO("robert@example.com", "Robert Doe", "Father", "9876543210");

        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 4, Rolename = "Parent" } }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Parent>().BuildMockDbSet().Object);

        
        var result = await _parentService.AddParentAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Name.Should().Be("Robert Doe");

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _parentRepoMock.Verify(r => r.AddAsync(It.IsAny<Parent>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddParentAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
    {
        
        var dto = new CreateParentDTO("robert@example.com", "Robert Doe", "Father", "9876543210");

        var existingUser = new User { Email = "robert@example.com" };
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User> { existingUser }.BuildMockDbSet().Object);

        
        Func<Task> action = async () => await _parentService.AddParentAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<DuplicateEntityException>().WithMessage("User with email/username 'robert@example.com' already exists.");
    }

    [Fact]
    public async Task GetParentByIdAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent>().BuildMockDbSet().Object);

        
        Func<Task> action = async () => await _parentService.GetParentByIdAsync(999, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Parent with identifier '999' was not found.");
    }
}
