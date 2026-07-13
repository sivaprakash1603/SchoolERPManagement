using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            _emailServiceMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task GetAllParentsAsync_ShouldFilterAndSort()
    {
        var parents = new List<Parent>
        {
            new Parent { Id = 1, Name = "Alice", Phonenumber = "123", User = new User { Email = "alice@test.com", Isactive = true } },
            new Parent { Id = 2, Name = "Bob", Phonenumber = "456", User = new User { Email = "bob@test.com", Isactive = false } }
        };
        _parentRepoMock.Setup(r => r.Query(true)).Returns(parents.BuildMockDbSet().Object);

        var request = new ParentQueryRequest { SearchQuery = "alice", Status = "Active", SortBy = "name", SortDirection = "desc", PageNumber = 1, PageSize = 10 };
        
        var result = await _parentService.GetAllParentsAsync(request, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetAllParentsAsync_DefaultSortAndNoChildren_ShouldReturnCorrectly()
    {
        var parents = new List<Parent>
        {
            new Parent { Id = 1, Name = "Alice", Phonenumber = "123", User = new User { Email = "alice@test.com" } }
        };
        _parentRepoMock.Setup(r => r.Query(true)).Returns(parents.BuildMockDbSet().Object);

        var request = new ParentQueryRequest { Status = "Inactive", PageNumber = 1, PageSize = 10 };
        
        var result = await _parentService.GetAllParentsAsync(request, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Relation.Should().Be("No linked children");
    }

    [Fact]
    public async Task GetParentByIdAsync_ValidId_ShouldReturnParent()
    {
        var parent = new Parent { Id = 1, Name = "Alice", Studentparents = new List<Studentparent> { new Studentparent { Student = new Student { Name = "Child" } } } };
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent> { parent }.BuildMockDbSet().Object);

        var result = await _parentService.GetParentByIdAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result.Relation.Should().Be("Child");
    }

    [Fact]
    public async Task GetParentByIdAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _parentService.GetParentByIdAsync(999, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetChildrenAsync_ValidParent_ShouldReturnChildren()
    {
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Parent { Id = 1 });
        var students = new List<Student> { new Student { Id = 1, Studentparents = { new Studentparent { Parentid = 1 } } } };
        _studentRepoMock.Setup(r => r.Query(true)).Returns(students.BuildMockDbSet().Object);
        var enrollments = new List<Studentenrollment> { new Studentenrollment { Studentid = 1, Class = new Class { Classname = "10" } } };
        _enrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.BuildMockDbSet().Object);

        var result = await _parentService.GetChildrenAsync(1, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().ClassName.Should().Be("10");
    }

    [Fact]
    public async Task GetChildrenAsync_InvalidParent_ShouldThrowEntityNotFoundException()
    {
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Parent?)null);

        Func<Task> action = async () => await _parentService.GetChildrenAsync(1, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddParentAsync_ValidData_ShouldCreateParentAndUser()
    {
        var dto = new CreateParentDTO("robert@example.com", "Robert Doe", "Father", "9876543210");

        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 4, Rolename = "Parent" } }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent>().BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(false)).Returns(new List<Parent>().BuildMockDbSet().Object);

        var result = await _parentService.AddParentAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Robert Doe");

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _parentRepoMock.Verify(r => r.AddAsync(It.IsAny<Parent>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddParentAsync_EmailFails_ShouldStillSucceed()
    {
        var dto = new CreateParentDTO("robert@example.com", "Robert Doe", "Father", "9876543210");
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role> { new Role { Id = 4, Rolename = "Parent" } }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent>().BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(false)).Returns(new List<Parent>().BuildMockDbSet().Object);
        
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SMTP Error"));

        var result = await _parentService.AddParentAsync(dto, CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddParentAsync_RoleNotFound_ShouldThrowEntityNotFoundException()
    {
        var dto = new CreateParentDTO("robert@example.com", "Robert", "Father", "123");
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        _roleRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Role>().BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(false)).Returns(new List<Parent>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _parentService.AddParentAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddParentAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
    {
        var dto = new CreateParentDTO("robert@example.com", "Robert Doe", "Father", "9876543210");
        var existingUser = new User { Email = "robert@example.com" };
        _userRepoMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User> { existingUser }.BuildMockDbSet().Object);
        _parentRepoMock.Setup(r => r.Query(false)).Returns(new List<Parent>().BuildMockDbSet().Object);

        Func<Task> action = async () => await _parentService.AddParentAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task GetParentIdByUserIdAsync_ShouldReturnId()
    {
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent> { new Parent { Id = 5, Userid = 10 } }.BuildMockDbSet().Object);
        var result = await _parentService.GetParentIdByUserIdAsync(10, CancellationToken.None);
        result.Should().Be(5);
    }

    [Fact]
    public async Task UpdateParentAsync_ValidData_ShouldUpdate()
    {
        var parent = new Parent { Id = 1, User = new User { Email = "old@test.com" } };
        _parentRepoMock.Setup(r => r.Query(false)).Returns(new List<Parent> { parent }.BuildMockDbSet().Object);

        var dto = new UpdateParentDTO("New Name", "new@test.com", "999");
        var result = await _parentService.UpdateParentAsync(1, dto, CancellationToken.None);

        result.Name.Should().Be("New Name");
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _parentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Parent>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateParentAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _parentRepoMock.Setup(r => r.Query(false)).Returns(new List<Parent>().BuildMockDbSet().Object);
        var dto = new UpdateParentDTO("New Name", "new@test.com", "999");

        Func<Task> action = async () => await _parentService.UpdateParentAsync(999, dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteParentAsync_ValidId_ShouldDeactivateUser()
    {
        _parentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Parent { Id = 1, Userid = 10 });
        var user = new User { Id = 10, Isactive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

        await _parentService.DeleteParentAsync(1, CancellationToken.None);

        user.Isactive.Should().BeFalse();
        _userRepoMock.Verify(r => r.UpdateAsync(user, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteParentAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        _parentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Parent?)null);

        Func<Task> action = async () => await _parentService.DeleteParentAsync(999, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetParentStatsAsync_ShouldReturnStats()
    {
        var parents = new List<Parent> { new Parent { Id = 1, Userid = 10 }, new Parent { Id = 2, Userid = 20 } };
        _parentRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(parents);

        var users = new List<User> { new User { Id = 10, Isactive = true }, new User { Id = 20, Isactive = false } };
        _userRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var result = await _parentService.GetParentStatsAsync(CancellationToken.None);

        result.TotalParents.Should().Be(2);
        result.ActiveParents.Should().Be(1);
        result.InactiveParents.Should().Be(1);
    }
}
