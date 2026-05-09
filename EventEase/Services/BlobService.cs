using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace EventEase.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName = "images";

        public BlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureBlob:ConnectionString"];
        }

        // ================= UPLOAD IMAGE =================
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        // ================= DELETE IMAGE =================
        public async Task DeleteFileAsync(string blobUrl)
        {
            if (string.IsNullOrEmpty(blobUrl))
                return;

            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                var uri = new Uri(blobUrl);

                // Extract file name from URL
                string blobName = Path.GetFileName(uri.LocalPath);

                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // optional logging (ignore failure so app doesn't crash)
            }
        }
    }
}