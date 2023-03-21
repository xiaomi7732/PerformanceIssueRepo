using Microsoft.Extensions.Options;
using OPI.Core.Models;
using OPI.Core.Utilities;
using OPI.Core.Validators;
using OPI.WebAPI.Contracts;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OPI.WebAPI.Services;

public class IssueRegistryService
{
    private readonly IRegistryBlobClient _storageClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SubstituteExtractor _substituteExtractor;
    private readonly SubstituteService _substituteService;
    private readonly ILogger _logger;

    public IssueRegistryService(
        IRegistryBlobClient storageClient,
        JsonSerializerOptions jsonSerializerOptions,
        IHttpContextAccessor httpContextAccessor,
        SubstituteExtractor substituteExtractor,
        SubstituteService substituteService,
        ILogger<IssueRegistryService> logger)
    {
        _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _substituteExtractor = substituteExtractor ?? throw new ArgumentNullException(nameof(substituteExtractor));
        _substituteService = substituteService ?? throw new ArgumentNullException(nameof(substituteService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Logic
    public async Task<PerfIssueRegisterEntry> RegisterNewIssueAsync(PerfIssueRegisterEntry registerEntry, RegistryEntryOptions options, CancellationToken cancellationToken)
    {
        // Any register entry shall have id of zero
        if (registerEntry.PermanentId is null || registerEntry.PermanentId == Guid.Empty)
        {
            registerEntry = registerEntry with
            {
                PermanentId = Guid.NewGuid(),
            };
        }

        await ValidateDataModelAsync(registerEntry, options, cancellationToken).ConfigureAwait(false);

        registerEntry = registerEntry.TrackCreate(_httpContextAccessor);
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
    public async Task<PerfIssueRegisterEntry> UpdateAsync(PerfIssueRegisterEntry newIssueRegistryItem, RegistryEntryOptions options, CancellationToken cancellationToken)
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

        await ValidateDataModelAsync(newIssueRegistryItem, options, cancellationToken).ConfigureAwait(false);

        newIssueRegistryItem = newIssueRegistryItem.TrackUpdate(entry, _httpContextAccessor);
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
        entry = entry.TrackUpdate(entry, _httpContextAccessor);
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
    private Task<PerfIssueRegisterEntry?> GetRegistryEntryAsync(Guid issueId, CancellationToken cancellationToken)
        => GetRegistryEntryAsync(new RegistryEntryName(issueId).Value, cancellationToken);

    private async IAsyncEnumerable<PerfIssueRegisterEntry> GetRegistryEntriesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (string blobName in _storageClient.ListBlobsAsync(RegistryEntryName.RegistryEntryPrefix + "/", cancellationToken))
        {
            PerfIssueRegisterEntry? item = await GetRegistryEntryAsync(blobName, cancellationToken);
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    private async Task<PerfIssueRegisterEntry?> GetRegistryEntryAsync(string blobName, CancellationToken cancellationToken)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            try
            {

                await _storageClient.DownloadStreamAsync(blobName, memoryStream, cancellationToken).ConfigureAwait(false);
            }
            catch (Azure.RequestFailedException ex) when (string.Equals(ex.ErrorCode, "BlobNotFound"))
            {
                _logger.LogInformation("Blob doesn't exist: {blobName}", blobName);
                return null;
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            PerfIssueRegisterEntry? entry = await JsonSerializer.DeserializeAsync<PerfIssueRegisterEntry>(memoryStream, _jsonSerializerOptions, cancellationToken);
            if (entry is null)
            {
                throw new InvalidCastException($"Can't deserialize blob: {blobName}");
            }
            return entry!;
        }
    }

    private async Task ValidateDataModelAsync(PerfIssueRegisterEntry entry, RegistryEntryOptions options, CancellationToken cancellationToken)
    {
        List<PerfIssue>? allItems = null;
        if (!options.AllowsDuplicatedHelpDocs || !options.AllowsNewSubstitutes)
        {
            // Prepare for the validation
            allItems = new();
            await foreach (PerfIssue item in GetAllIssueItemsAsync(activeState: null, cancellationToken).ConfigureAwait(false))
            {
                allItems.Add(item);
            }
        }

        if (allItems is null)
        {
            throw new InvalidOperationException("All items shouldn't be null.");
        }

        // Unless requested by the client, do not allow duplicated help docs.
        if (!options.AllowsDuplicatedHelpDocs)
        {
            SameHelpLinkValidator validator = new(entry, allItems);
            if (!await validator.ValidateAsync(cancellationToken))
            {
                throw new DataModelValidationException(validator.Reason);
            }
        }

        if (!options.AllowsNewSubstitutes)
        {
            IEnumerable<string> existingSubstitutes = await _substituteService.ListSubstitutesAsync(
                (stoppingToken) => Task.FromResult(allItems.AsEnumerable()),
                cancellationToken).ConfigureAwait(false);

            NewSubstituteValidator validator = new(entry, existingSubstitutes, _substituteExtractor);
            if (!await validator.ValidateAsync(cancellationToken))
            {
                throw new DataModelValidationException(validator.Reason);
            }
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