using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SchoolERPManagementBLLibrary.Interfaces;
using Microsoft.Extensions.Logging;

namespace SchoolERPManagementBLLibrary.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration["AzureBlobStorage:ConnectionString"] ?? configuration["AzureBlobStorage--ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("AzureBlobStorage:ConnectionString is missing from configuration.");
            throw new ArgumentNullException(nameof(connectionString), "Azure Blob Storage connection string is not configured.");
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "school-erp-uploads";
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.", nameof(file));
        }

        try
        {
            // Get a reference to a container and then create it if it doesn't exist
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobPath = string.IsNullOrEmpty(folderName) ? fileName : $"{folderName}/{fileName}";

            BlobClient blobClient = containerClient.GetBlobClient(blobPath);

            using var stream = file.OpenReadStream();
            var blobUploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            };

            await blobClient.UploadAsync(stream, blobUploadOptions, cancellationToken);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to Azure Blob Storage.", file.FileName);
            throw;
        }
    }

    public void DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        try
        {
            var uri = new Uri(fileUrl);
            var blobPath = uri.LocalPath; // e.g. /school-erp-uploads/documents/xxx.pdf

            // Remove the container name from the path if present
            var prefixToRemove = $"/{_containerName}/";
            if (blobPath.StartsWith(prefixToRemove, StringComparison.OrdinalIgnoreCase))
            {
                blobPath = blobPath.Substring(prefixToRemove.Length);
            }

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobPath);

            blobClient.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl} from Azure Blob Storage.", fileUrl);
        }
    }
}
