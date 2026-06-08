using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Homework;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class HomeworkServiceTests
{
    private readonly Mock<IRepository<int, Homework>> _homeworkRepoMock;
    private readonly Mock<IRepository<int, Homeworksubmission>> _submissionRepoMock;
    private readonly Mock<IRepository<int, Subject>> _subjectRepoMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly HomeworkService _homeworkService;

    public HomeworkServiceTests()
    {
        _homeworkRepoMock = new Mock<IRepository<int, Homework>>();
        _submissionRepoMock = new Mock<IRepository<int, Homeworksubmission>>();
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();

        _homeworkService = new HomeworkService(
            _homeworkRepoMock.Object,
            _submissionRepoMock.Object,
            _subjectRepoMock.Object,
            _teacherRepoMock.Object,
            _classRepoMock.Object,
            _studentRepoMock.Object,
            _fileStorageServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateHomeworkAsync_ValidData_ShouldCreateHomework()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var dto = new CreateHomeworkDTO(1, 1, 1, "Math Assignment", "Do exercises 1 to 10", mockFile.Object, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));

        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });

        _fileStorageServiceMock.Setup(f => f.UploadFileAsync(mockFile.Object, "homeworks", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/homeworks/file.pdf");

        // Act
        var result = await _homeworkService.CreateHomeworkAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Math Assignment");
        result.AttachmentUrl.Should().Be("/uploads/homeworks/file.pdf");

        _homeworkRepoMock.Verify(r => r.AddAsync(It.IsAny<Homework>(), true, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(f => f.UploadFileAsync(mockFile.Object, "homeworks", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateHomeworkAsync_InvalidSubject_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new CreateHomeworkDTO(999, 1, 1, "Math Assignment", "Do exercises 1 to 10", null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));

        _subjectRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Subject?)null);

        // Act
        Func<Task> action = async () => await _homeworkService.CreateHomeworkAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Subject with identifier '999' was not found.");
    }

    [Fact]
    public async Task SubmitHomeworkAsync_NewSubmission_ShouldAddSubmission()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var dto = new HomeworkSubmissionDTO(1, 1, mockFile.Object);

        _homeworkRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Homework { Id = 1 });
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        _fileStorageServiceMock.Setup(f => f.UploadFileAsync(mockFile.Object, "homeworksubmissions", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/homeworksubmissions/answer.pdf");

        _submissionRepoMock.Setup(r => r.Query(true)).Returns(new List<Homeworksubmission>().AsQueryable().BuildMock());

        // Act
        var result = await _homeworkService.SubmitHomeworkAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UploadedFileUrl.Should().Be("/uploads/homeworksubmissions/answer.pdf");

        _submissionRepoMock.Verify(r => r.AddAsync(It.IsAny<Homeworksubmission>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateHomeworkAsync_ValidData_ShouldUpdateSubmission()
    {
        // Arrange
        var dto = new EvaluateHomeworkDTO(1, 90.5m, "Good job", "Reviewed");

        var submission = new Homeworksubmission { Id = 1, Homeworkid = 1, Studentid = 1 };
        _submissionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(submission);

        // Act
        var result = await _homeworkService.EvaluateHomeworkAsync(dto, CancellationToken.None);

        // Assert
        result.Marks.Should().Be(90.5m);
        result.VerificationStatus.Should().Be("Reviewed");
        result.Remarks.Should().Be("Good job");

        _submissionRepoMock.Verify(r => r.UpdateAsync(submission, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateHomeworkAsync_InvalidSubmission_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new EvaluateHomeworkDTO(999, 90.5m, "Good job", "Reviewed");
        _submissionRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Homeworksubmission?)null);

        // Act
        Func<Task> action = async () => await _homeworkService.EvaluateHomeworkAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Homework submission with identifier '999' was not found.");
    }
}
