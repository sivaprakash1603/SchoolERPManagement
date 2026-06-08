using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Subject;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class SubjectServiceTests
{
    private readonly Mock<IRepository<int, Subject>> _subjectRepoMock;
    private readonly SubjectService _subjectService;

    public SubjectServiceTests()
    {
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _subjectService = new SubjectService(_subjectRepoMock.Object);
    }

    [Fact]
    public async Task CreateSubjectAsync_ValidData_ShouldCreateSubject()
    {
        // Arrange
        var dto = new CreateSubjectDTO("Mathematics");
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject>().AsQueryable().BuildMock());

        // Act
        var result = await _subjectService.CreateSubjectAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SubjectName.Should().Be("Mathematics");
        
        _subjectRepoMock.Verify(r => r.AddAsync(It.IsAny<Subject>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubjectAsync_DuplicateName_ShouldThrowDuplicateEntityException()
    {
        // Arrange
        var dto = new CreateSubjectDTO("Mathematics");
        var existing = new Subject { Subjectname = "mathematics" };
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject> { existing }.AsQueryable().BuildMock());

        // Act
        Func<Task> action = async () => await _subjectService.CreateSubjectAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DuplicateEntityException>().WithMessage("Subject with SubjectName 'Mathematics' already exists.");
    }

    [Fact]
    public async Task GetSubjectByIdAsync_ValidId_ShouldReturnSubject()
    {
        // Arrange
        var subject = new Subject { Id = 1, Subjectname = "Physics" };
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);

        // Act
        var result = await _subjectService.GetSubjectByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SubjectName.Should().Be("Physics");
    }

    [Fact]
    public async Task GetSubjectByIdAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _subjectRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Subject?)null);

        // Act
        Func<Task> action = async () => await _subjectService.GetSubjectByIdAsync(999, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Subject with identifier '999' was not found.");
    }

    [Fact]
    public async Task UpdateSubjectAsync_ValidData_ShouldUpdateSubject()
    {
        // Arrange
        var subject = new Subject { Id = 1, Subjectname = "OldName" };
        var dto = new CreateSubjectDTO("NewName");
        
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject>().AsQueryable().BuildMock());

        // Act
        var result = await _subjectService.UpdateSubjectAsync(1, dto, CancellationToken.None);

        // Assert
        result.SubjectName.Should().Be("NewName");
        _subjectRepoMock.Verify(r => r.UpdateAsync(subject, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSubjectAsync_ValidId_ShouldDeleteSubject()
    {
        // Arrange
        var subject = new Subject { Id = 1, Subjectname = "Physics" };
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);

        // Act
        await _subjectService.DeleteSubjectAsync(1, CancellationToken.None);

        // Assert
        _subjectRepoMock.Verify(r => r.DeleteAsync(subject, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
