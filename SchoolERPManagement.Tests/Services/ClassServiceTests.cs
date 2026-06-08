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
    private readonly ClassService _classService;

    public ClassServiceTests()
    {
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();

        _classService = new ClassService(_classRepoMock.Object, _teacherRepoMock.Object);
    }

    [Fact]
    public async Task GetAllClassesAsync_ShouldReturnListOfClasses()
    {
        // Arrange
        var classes = new List<Class>
        {
            new Class { Id = 1, Classname = "10", Section = "A" },
            new Class { Id = 2, Classname = "10", Section = "B" }
        };

        _classRepoMock.Setup(r => r.Query(true)).Returns(classes.AsQueryable().BuildMock());

        // Act
        var result = await _classService.GetAllClassesAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Classname.Should().Be("10");
        result.First().Section.Should().Be("A");
    }

    [Fact]
    public async Task CreateClassAsync_ValidData_ShouldCreateClass()
    {
        // Arrange
        var dto = new CreateClassDTO("10", "A", 1);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });

        // Act
        var result = await _classService.CreateClassAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Classname.Should().Be("10");
        result.Section.Should().Be("A");
        
        _classRepoMock.Verify(r => r.AddAsync(It.IsAny<Class>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClassAsync_InvalidTeacher_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new CreateClassDTO("10", "A", 999);
        _teacherRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Teacher?)null);

        // Act
        Func<Task> action = async () => await _classService.CreateClassAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Teacher with identifier '999' was not found.");
    }
}
