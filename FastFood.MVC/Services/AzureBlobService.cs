using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace FastFood.MVC.Services
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobService> _logger;

        public AzureBlobService(IConfiguration configuration, ILogger<AzureBlobService> logger)
        {
            _connectionString = configuration["AzureBlobStorage:ConnectionString"]!;
            _containerName = configuration["AzureBlobStorage:ContainerName"]!;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            _logger.LogInformation("File uploaded successfully. Blob URL: {BlobUrl}", blobClient.Uri.ToString());
            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string blobUrl)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            var blobName = new Uri(blobUrl).Segments.Last();
            await blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            _logger.LogInformation("Blob deleted successfully: {BlobName}", blobName);
        }
    }
}