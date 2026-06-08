using Microsoft.AspNetCore.Http;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default);
    void DeleteFile(string fileUrl);
}
