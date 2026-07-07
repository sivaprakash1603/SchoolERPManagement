using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class ExamServiceTests
{
    private readonly Mock<IRepository<int, Exam>> _examRepoMock;
    private readonly Mock<IRepository<int, Examresult>> _examResultRepoMock;
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly Mock<IRepository<int, Subject>> _subjectRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _studentEnrollmentRepoMock;
    private readonly Mock<IRepository<int, Examschedule>> _examScheduleRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly ExamService _examService;

    public ExamServiceTests()
    {
        _examRepoMock = new Mock<IRepository<int, Exam>>();
        _examResultRepoMock = new Mock<IRepository<int, Examresult>>();
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _studentEnrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _examScheduleRepoMock = new Mock<IRepository<int, Examschedule>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _notificationServiceMock = new Mock<INotificationService>();

        _examService = new ExamService(
            _examRepoMock.Object,
            _examResultRepoMock.Object,
            _academicYearRepoMock.Object,
            _subjectRepoMock.Object,
            _studentRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _examScheduleRepoMock.Object,
            _classRepoMock.Object,
            _notificationServiceMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task CreateExamAsync_ValidData_ShouldCreateExam()
    {
        
        var dto = new CreateExamDTO("Final Exams 2025", 1);
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Academicyear { Id = 1 });

        
        var result = await _examService.CreateExamAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Examname.Should().Be("Final Exams 2025");
        _examRepoMock.Verify(r => r.AddAsync(It.IsAny<Exam>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateExamAsync_InvalidAcademicYear_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new CreateExamDTO("Final Exams 2025", 999);
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Academicyear?)null);

        
        Func<Task> action = async () => await _examService.CreateExamAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Academic year with identifier '999' was not found.");
    }

    [Fact]
    public async Task PublishResultAsync_NewResult_ShouldAddResult()
    {
        
        var dto = new PublishResultDTO(1, 1, 1, 95.5m, null);
        
        _examRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Exam { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        _examResultRepoMock.Setup(r => r.Query(true)).Returns(new List<Examresult>().BuildMockDbSet().Object);

        
        var result = await _examService.PublishResultAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Marks.Should().Be(95.5m);
        _examResultRepoMock.Verify(r => r.AddAsync(It.IsAny<Examresult>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishResultAsync_ExistingResult_ShouldUpdateResult()
    {
        
        var dto = new PublishResultDTO(1, 1, 1, 98.0m, "url");
        
        _examRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Exam { Id = 1 });
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Subject { Id = 1 });
        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });

        var existingResult = new Examresult { Id = 1, Examid = 1, Subjectid = 1, Studentid = 1, Marks = 95.5m };
        _examResultRepoMock.Setup(r => r.Query(true)).Returns(new List<Examresult> { existingResult }.BuildMockDbSet().Object);

        
        var result = await _examService.PublishResultAsync(dto, CancellationToken.None);

        
        result.Marks.Should().Be(98.0m);
        _examResultRepoMock.Verify(r => r.UpdateAsync(existingResult, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStudentResultsAsync_ShouldReturnResults()
    {
        
        var results = new List<Examresult>
        {
            new Examresult { Id = 1, Studentid = 1, Marks = 85.0m },
            new Examresult { Id = 2, Studentid = 1, Marks = 90.0m }
        };

        _examResultRepoMock.Setup(r => r.Query(true)).Returns(results.BuildMockDbSet().Object);

        // Act
        var result = await _examService.GetStudentResultsAsync(1, 1, "Admin", CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Marks.Should().Be(90.0m); 
    }
}
