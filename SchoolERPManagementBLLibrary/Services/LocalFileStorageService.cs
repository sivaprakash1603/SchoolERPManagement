using Microsoft.AspNetCore.Http;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    public async Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.", nameof(file));
        }

        var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            throw new SchoolERPManagementBLLibrary.Exceptions.BusinessRuleException($"File extension '{extension}' is not allowed.");
        }

        var uploadRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", folderName);
        if (!Directory.Exists(uploadRoot))
        {
            Directory.CreateDirectory(uploadRoot);
        }

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadRoot, fileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"/uploads/{folderName}/{fileName}";
    }

    public void DeleteFile(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return;

        
        
        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
