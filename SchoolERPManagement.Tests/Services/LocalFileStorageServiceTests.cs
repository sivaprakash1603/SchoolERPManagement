using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SchoolERPManagementBLLibrary.Services;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class LocalFileStorageServiceTests
{
    private readonly LocalFileStorageService _fileStorageService;

    public LocalFileStorageServiceTests()
    {
        _fileStorageService = new LocalFileStorageService(new Moq.Mock<AutoMapper.IMapper>().Object);
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ShouldReturnFilePathAndSaveFile()
    {
        
        var content = "This is a dummy file";
        var fileName = "dummy.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((s, _) => stream.CopyTo(s))
                .Returns(Task.CompletedTask);

        var folderName = "test-uploads";

        
        var result = await _fileStorageService.UploadFileAsync(fileMock.Object, folderName, CancellationToken.None);

        
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith($"/uploads/{folderName}/");
        result.Should().EndWith(".txt");

        
        _fileStorageService.DeleteFile(result);
    }

    [Fact]
    public async Task UploadFileAsync_NullFile_ShouldThrowArgumentException()
    {
        
        IFormFile? nullFile = null;

        
        Func<Task> action = async () => await _fileStorageService.UploadFileAsync(nullFile!, "test", CancellationToken.None);

        
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*File is empty or null.*");
    }
}
