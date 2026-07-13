using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IRepository<int, Studentdocument>> _studentDocRepoMock;
    private readonly Mock<IRepository<int, Teacherdocument>> _teacherDocRepoMock;
    private readonly Mock<IRepository<int, Parentdocument>> _parentDocRepoMock;
    private readonly Mock<IRepository<int, Student>> _studentRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _studentEnrollmentRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IDocumentVerificationStrategy> _strategyMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _studentDocRepoMock = new Mock<IRepository<int, Studentdocument>>();
        _teacherDocRepoMock = new Mock<IRepository<int, Teacherdocument>>();
        _parentDocRepoMock = new Mock<IRepository<int, Parentdocument>>();
        _studentRepoMock = new Mock<IRepository<int, Student>>();
        _studentEnrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _strategyMock = new Mock<IDocumentVerificationStrategy>();

        var notificationServiceMock = new Mock<INotificationService>();

        _documentService = new DocumentService(
            _studentDocRepoMock.Object,
            _teacherDocRepoMock.Object,
            _parentDocRepoMock.Object,
            _studentRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _classRepoMock.Object,
            _fileStorageServiceMock.Object,
            _teacherRepoMock.Object,
            new List<IDocumentVerificationStrategy> { _strategyMock.Object },
            notificationServiceMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task UploadStudentDocumentAsync_ValidData_ShouldUploadAndCreateDocument()
    {
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("doc.pdf");
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        _studentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1 });
        _fileStorageServiceMock.Setup(f => f.UploadFileAsync(mockFile.Object, "studentdocuments", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/studentdocuments/doc.pdf");

        // Act
        var result = await _documentService.UploadStudentDocumentAsync(mockFile.Object, 1, null, "Student", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("application/pdf", result.DocumentType);
    }

    [Fact]
    public async Task UploadStudentDocumentAsync_ShouldThrowEntityNotFoundException_WhenStudentDoesNotExist()
    {
        // Arrange
        var studentId = 1;
        var fileMock = new Mock<IFormFile>();

        _studentRepoMock.Setup(repo => repo.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => 
            _documentService.UploadStudentDocumentAsync(fileMock.Object, studentId, null, "Student", CancellationToken.None));
    }

    [Fact]
    public async Task UploadTeacherDocumentAsync_ShouldReturnResponse_WhenValid()
    {
        // Arrange
        var teacherId = 1;
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.pdf");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        _teacherRepoMock.Setup(repo => repo.GetByIdAsync(teacherId))
            .ReturnsAsync(new Teacher { Id = teacherId });

        _fileStorageServiceMock.Setup(service => service.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/teacherdocuments/test.pdf");

        // Act
        var result = await _documentService.UploadTeacherDocumentAsync(fileMock.Object, teacherId, null, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.BlobUrl.Should().Be("/uploads/teacherdocuments/test.pdf");

        _teacherDocRepoMock.Verify(r => r.AddAsync(It.IsAny<Teacherdocument>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldDeleteFromRepoAndStorage()
    {
        
        var blobUrl = "/uploads/studentdocuments/doc.pdf";
        var studentDoc = new Studentdocument { Id = 1, Bloburl = blobUrl };
        
        _studentDocRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentdocument> { studentDoc }.BuildMockDbSet().Object);
        _teacherDocRepoMock.Setup(r => r.Query(true)).Returns(new List<Teacherdocument>().BuildMockDbSet().Object);

        
        await _documentService.DeleteDocumentAsync(blobUrl, CancellationToken.None);

        
        _studentDocRepoMock.Verify(r => r.DeleteAsync(studentDoc, true, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(f => f.DeleteFile(blobUrl), Times.Once);
    }

    [Fact]
    public async Task VerifyDocumentAsync_ValidStrategy_ShouldCallStrategyVerify()
    {
        
        var dto = new VerifyDocumentDTO("student", 1, "Verified");
        _strategyMock.Setup(s => s.CanHandle("student")).Returns(true);
        _strategyMock.Setup(s => s.VerifyAsync(dto, 1, "Admin", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var studentDoc = new Studentdocument { Id = 1, Student = new Student { Userid = 2 } };
        _studentDocRepoMock.Setup(r => r.Query(true)).Returns(new List<Studentdocument> { studentDoc }.BuildMockDbSet().Object);

        
        await _documentService.VerifyDocumentAsync(dto, 1, "Admin", CancellationToken.None);

        
        _strategyMock.Verify(s => s.VerifyAsync(dto, 1, "Admin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyDocumentAsync_NoStrategyFound_ShouldThrowBusinessRuleException()
    {
        
        var dto = new VerifyDocumentDTO("unknown", 1, "Verified");
        _strategyMock.Setup(s => s.CanHandle("unknown")).Returns(false);

        
        Func<Task> action = async () => await _documentService.VerifyDocumentAsync(dto, 1, "Admin", CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Invalid document type.");
    }
}
