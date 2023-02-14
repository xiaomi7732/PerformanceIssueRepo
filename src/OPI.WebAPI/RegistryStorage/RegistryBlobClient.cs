using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace OPI.WebAPI.RegistryStorage;

internal sealed class RegistryBlobClient : IRegistryBlobClient
{
    private readonly RegistryStorageOptions _options;
    private readonly RegistryStorageCredential _credential;
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;


    public RegistryBlobClient(
        IOptions<RegistryStorageOptions> options,
        RegistryStorageCredential credential,
        ILogger<RegistryBlobClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));

        _blobServiceClient = new BlobServiceClient(_options.BlobServiceUri, _credential);
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        _logger.LogInformation("Checking container exists: {containerName}", _options.ContainerName);
        _blobContainerClient.CreateIfNotExists();
    }

    public async Task ReplaceAsync(string blobName, Stream data, CancellationToken cancellationToken)
    {
        bool overwrite = true;
        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

        BlobUploadOptions blobUploadOptions = new BlobUploadOptions()
        {
            HttpHeaders = new BlobHttpHeaders()
            {
                ContentType = MediaTypeNames.Application.Json,
                ContentEncoding = Encoding.UTF8.WebName,
                ContentLanguage = "en-us"
            },
            Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = new ETag("*") },

        };

        await blobClient.UploadAsync(data, blobUploadOptions, cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<string> ListBlobsAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning blob for container: {containerName}, by prefix: {prefix}", _blobContainerClient.Name, prefix);
        var resultSegment = _blobContainerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/").AsPages(default);

        // Enumerate the blobs returned for each page.
        await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
        {
            // A hierarchical listing may return both virtual directories and blobs.
            foreach (BlobHierarchyItem item in blobPage.Values)
            {
                if (item.IsPrefix)
                {
                    continue;
                }
                yield return item.Blob.Name;
            }
        }
    }

    public async Task DownloadStreamAsync(string blobName, Stream outputStream, CancellationToken cancellationToken)
    {
        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);
        await blobClient.DownloadToAsync(outputStream, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken)
    {
        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}