public interface IRegistryBlobClient
{
    Task CreateContainerIfNotExistsAsync(CancellationToken cancellationToken);
    Task DownloadStreamAsync(string blobName, Stream outputStream, CancellationToken cancellationToken);
    Task ReplaceAsync(string blobName, Stream data, CancellationToken cancellationToken);
    IAsyncEnumerable<string> ListBlobsAsync(string prefix, CancellationToken cancellationToken);
    Task<bool> DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken);
}
