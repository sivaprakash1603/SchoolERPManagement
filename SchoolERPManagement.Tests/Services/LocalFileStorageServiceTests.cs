using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly LocalFileStorageService _service;
    private readonly string _uploadRoot;

    public LocalFileStorageServiceTests()
    {
        _service = new LocalFileStorageService();
        _uploadRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", "test");
        if (Directory.Exists(_uploadRoot))
        {
            Directory.Delete(_uploadRoot, true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_uploadRoot))
        {
            Directory.Delete(_uploadRoot, true);
        }
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ShouldUploadAndReturnUrl()
    {
        var mockFile = new Mock<IFormFile>();
        var content = "Hello World from a Fake File";
        var fileName = "test.pdf";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.Length).Returns(ms.Length);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream, token));

        var result = await _service.UploadFileAsync(mockFile.Object, "test", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().StartWith("/uploads/test/");
        result.Should().EndWith(".pdf");

        // Verify file exists on disk
        var filePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", result.TrimStart('/'));
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadFileAsync_NullFile_ShouldThrowArgumentException()
    {
        Func<Task> act = async () => await _service.UploadFileAsync(null, "test", CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadFileAsync_EmptyFile_ShouldThrowArgumentException()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(_ => _.Length).Returns(0);

        Func<Task> act = async () => await _service.UploadFileAsync(mockFile.Object, "test", CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadFileAsync_InvalidExtension_ShouldThrowBusinessRuleException()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(_ => _.FileName).Returns("test.txt");
        mockFile.Setup(_ => _.Length).Returns(100);

        Func<Task> act = async () => await _service.UploadFileAsync(mockFile.Object, "test", CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("File extension '.txt' is not allowed.");
    }

    [Fact]
    public void DeleteFile_ValidUrl_ShouldDeleteFile()
    {
        var uploadRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", "test");
        Directory.CreateDirectory(uploadRoot);
        var filePath = Path.Combine(uploadRoot, "delete_me.pdf");
        File.WriteAllText(filePath, "dummy content");
        File.Exists(filePath).Should().BeTrue();

        var fileUrl = "/uploads/test/delete_me.pdf";

        _service.DeleteFile(fileUrl);

        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_EmptyUrl_ShouldDoNothing()
    {
        _service.DeleteFile(null);
        _service.DeleteFile("");
        _service.DeleteFile(" ");
    }
}
