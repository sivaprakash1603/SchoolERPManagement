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

        _documentService = new DocumentService(
            _studentDocRepoMock.Object,
            _teacherDocRepoMock.Object,
            _parentDocRepoMock.Object,
            _studentRepoMock.Object,
            _studentEnrollmentRepoMock.Object,
            _classRepoMock.Object,
            _fileStorageServiceMock.Object,
            _teacherRepoMock.Object,
            new List<IDocumentVerificationStrategy> { _strategyMock.Object }
        ,
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

        
        var result = await _documentService.UploadStudentDocumentAsync(mockFile.Object, 1, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.DocumentType.Should().Be("application/pdf");
        result.BlobUrl.Should().Be("/uploads/studentdocuments/doc.pdf");

        _studentDocRepoMock.Verify(r => r.AddAsync(It.IsAny<Studentdocument>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadStudentDocumentAsync_InvalidStudent_ShouldThrowEntityNotFoundException()
    {
        
        var mockFile = new Mock<IFormFile>();
        _studentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Student?)null);

        
        Func<Task> action = async () => await _documentService.UploadStudentDocumentAsync(mockFile.Object, 999, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Student with identifier '999' was not found.");
    }

    [Fact]
    public async Task UploadTeacherDocumentAsync_ValidData_ShouldUploadAndCreateDocument()
    {
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("doc.pdf");
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        _teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher { Id = 1 });
        _fileStorageServiceMock.Setup(f => f.UploadFileAsync(mockFile.Object, "teacherdocuments", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/teacherdocuments/doc.pdf");

        
        var result = await _documentService.UploadTeacherDocumentAsync(mockFile.Object, 1, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.BlobUrl.Should().Be("/uploads/teacherdocuments/doc.pdf");

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
