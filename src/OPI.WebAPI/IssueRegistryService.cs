using Microsoft.Extensions.Options;
using OPI.Core.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OPI.WebAPI.Services;

public class IssueRegistryService
{
    private readonly IssueServiceOptions _options;
    private readonly IRegistryBlobClient _storageClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger _logger;

    public IssueRegistryService(
        IRegistryBlobClient storageClient,
        IOptions<IssueServiceOptions> options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<IssueRegistryService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Logic
    public async Task<PerfIssueRegisterEntry> RegisterNewIssueAsync(PerfIssueRegisterEntry registerEntry, CancellationToken cancellationToken)
    {
        // Any register entry shall have id of zero
        if (registerEntry.PermanentId is null || registerEntry.PermanentId == Guid.Empty)
        {
            registerEntry = registerEntry with
            {
                PermanentId = Guid.NewGuid(),
            };
        }

        await SaveRegistryItemAsync(registerEntry, cancellationToken).ConfigureAwait(false);
        return registerEntry;
    }

    // Retrieve

    /// <summary>
    /// Gets an item by id.
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> GetRegisteredItemAsync(Guid id, CancellationToken cancellationToken)
    {
        return await GetRegistryEntryAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets all registered items.
    /// </summary>
    public IAsyncEnumerable<PerfIssueRegisterEntry> GetRegisteredIssuesAsync(CancellationToken cancellationToken)
    {
        return GetRegistryEntriesAsync(cancellationToken);
    }


    // Update
    /// <summary>
    /// Updates the perf issue registry entry. Returns the new result.
    /// Throws IndexOutOfRange exception when not found by id.
    /// </summary>
    public async Task<PerfIssueRegisterEntry> UpdateAsync(PerfIssueRegisterEntry newIssueRegistryItem, CancellationToken cancellationToken)
    {
        if (newIssueRegistryItem.PermanentId is null || newIssueRegistryItem.PermanentId == Guid.Empty)
        {
            throw new InvalidOperationException($"Id {Guid.Empty} is invalid.");
        }

        PerfIssueRegisterEntry? entry = await GetRegisteredItemAsync(newIssueRegistryItem.PermanentId.Value, cancellationToken).ConfigureAwait(false);
        if (entry is null)
        {
            throw new InvalidOperationException($"Target entry by id {newIssueRegistryItem.PermanentId} does not exist.");
        }

        await SaveRegistryItemAsync(newIssueRegistryItem, cancellationToken);
        return newIssueRegistryItem;
    }

    /// <summary>
    /// Update activation of an registry item. Return the updated PerfIssueRegistryEntry when updated. Null when item is not updated.
    /// Throws exception when item is not there.
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> FlipActivationAsync(Guid permanentId, CancellationToken cancellationToken)
    {
        PerfIssueRegisterEntry? entry = await GetRegisteredItemAsync(permanentId, cancellationToken).ConfigureAwait(false);
        if (entry is null)
        {
            throw new InvalidOperationException($"Target issue by id {permanentId} does not exist.");
        }
        entry = entry with { IsActive = !entry.IsActive };
        await SaveRegistryItemAsync(entry, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    // Delete
    public async Task<bool> DeleteAnIssueAsync(Guid permanentId, CancellationToken cancellationToken)
    {
        string blobName = new RegistryEntryName(permanentId).Value;
        return await _storageClient.DeleteIfExistsAsync(blobName, cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<PerfIssueItem> GetAllIssueItemsAsync(bool? activeState, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry entry in GetRegistryEntriesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (activeState is null || entry.IsActive == activeState)
            {
                PerfIssueItem newItem = new PerfIssueItem(entry);
                yield return newItem;
            }
        }
    }

    // Data access below
    private Task<PerfIssueRegisterEntry> GetRegistryEntryAsync(Guid issueId, CancellationToken cancellationToken)
        => GetRegistryEntryAsync(new RegistryEntryName(issueId).Value, cancellationToken);

    private async IAsyncEnumerable<PerfIssueRegisterEntry> GetRegistryEntriesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (string blobName in _storageClient.ListBlobsAsync(RegistryEntryName.RegistryEntryPrefix + "/", cancellationToken))
        {
            yield return await GetRegistryEntryAsync(blobName, cancellationToken);
        }
    }

    private async Task<PerfIssueRegisterEntry> GetRegistryEntryAsync(string blobName, CancellationToken cancellationToken)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            await _storageClient.DownloadStreamAsync(blobName, memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            PerfIssueRegisterEntry? entry = await JsonSerializer.DeserializeAsync<PerfIssueRegisterEntry>(memoryStream, _jsonSerializerOptions, cancellationToken);
            if (entry is null)
            {
                throw new InvalidCastException($"Can't deserialize blob: {blobName}");
            }
            return entry!;
        }
    }

    internal async Task SaveRegistryItemAsync(PerfIssueRegisterEntry entry, CancellationToken cancellationToken)
    {
        if (entry.PermanentId is null)
        {
            throw new InvalidOperationException("Can't persistent a perf issue without a durable guid.");
        }

        using (Stream inputStream = new MemoryStream())
        {
            await JsonSerializer.SerializeAsync(inputStream, entry, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            inputStream.Seek(0, SeekOrigin.Begin);
            await _storageClient.ReplaceAsync(new RegistryEntryName(entry.PermanentId.Value).Value, inputStream, cancellationToken).ConfigureAwait(false);
        }
    }
}