using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Logistics.Domain.Services;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.EF.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobStorageOptions _options;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, IOptions<AzureBlobStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders,
            Conditions = null
        }, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public async Task<BlobProperties> GetPropertiesAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        var properties = response.Value;

        return new BlobProperties(
            properties.ContentType,
            properties.ContentLength,
            properties.LastModified,
            properties.ETag.ToString()
        );
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }
}

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultContainer { get; set; } = "documents";
}