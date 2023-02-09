using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OPI.Core.Models;

namespace OPI.WebAPI.Services;

public class IssueRegistryService
{
    private readonly IssueServiceOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger _logger;

    public IssueRegistryService(
        IOptions<IssueServiceOptions> options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<IssueRegistryService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
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

        List<PerfIssueRegisterEntry> result = new List<PerfIssueRegisterEntry>();
        await foreach (PerfIssueRegisterEntry item in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(item);
        }
        result.Add(registerEntry);

        await SaveAllPerfIssueAsync(result, cancellationToken).ConfigureAwait(false);
        return registerEntry;
    }

    // Retrieve

    /// <summary>
    /// Gets an item by id.
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> GetRegisteredItem(Guid id, CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry item in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item?.PermanentId == id)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all registered items.
    /// </summary>
    public IAsyncEnumerable<PerfIssueRegisterEntry> GetRegisteredIssuesAsync(CancellationToken cancellationToken)
    {
        return GetAllPerfIssuesAsync(cancellationToken);
    }


    // Update
    /// <summary>
    /// Updates the perf issue registry entry. Returns the new result.
    /// Throws IndexOutOfRange exception when not found by id.
    /// </summary>
    public async Task<PerfIssueRegisterEntry> UpdateAsync(PerfIssueRegisterEntry newIssueRegistryItem, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> results = new List<PerfIssueRegisterEntry>();
        bool found = false;
        await foreach (PerfIssueRegisterEntry entry in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (entry.PermanentId != newIssueRegistryItem.PermanentId)
            {
                results.Add(entry);
                continue;
            }

            results.Add(newIssueRegistryItem);
            found = true;
        }

        if (!found)
        {
            throw new IndexOutOfRangeException($"Can't find an entry by id: {newIssueRegistryItem.PermanentId}");
        }

        await SaveAllPerfIssueAsync(results, cancellationToken).ConfigureAwait(false);
        return newIssueRegistryItem;
    }

    /// <summary>
    /// Update activation of an registry item. Return the updated PerfIssueRegistryEntry when updated. Null when item is not updated.
    /// Throws exception when item is not there.
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> FlipActivationAsync(Guid permanentId, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> result = new List<PerfIssueRegisterEntry>();
        PerfIssueRegisterEntry? updated = null;

        await foreach (PerfIssueRegisterEntry entry in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            // No hit;
            if (entry.PermanentId != permanentId)
            {
                result.Add(entry);
                continue;
            }

            // Hit;
            updated = entry with
            {
                IsActive = !entry.IsActive,
            };
            result.Add(updated);
        }

        if (updated is null)
        {
            throw new IndexOutOfRangeException($"No issue entry found by id of {permanentId}");
        }

        // Update happened
        await SaveAllPerfIssueAsync(result, cancellationToken).ConfigureAwait(false);
        return updated;
    }

    // Delete
    public async Task<bool> DeleteAnIssueAsync(Guid permanentId, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> itemsToKeep = new List<PerfIssueRegisterEntry>();
        bool deleted = false;
        await foreach (PerfIssueRegisterEntry item in GetAllPerfIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item.PermanentId == permanentId)
            {
                deleted = true;
            }
            else
            {
                itemsToKeep.Add(item);
            }
        }

        // Persistent
        await SaveAllPerfIssueAsync(itemsToKeep, cancellationToken);
        return deleted;
    }

    public async IAsyncEnumerable<PerfIssueItem> GetAllIssueItems(bool? activeState, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry entry in GetAllPerfIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (activeState is null || entry.IsActive == activeState)
            {
                PerfIssueItem newItem = new PerfIssueItem(entry);
                yield return newItem;
            }
        }
    }

    // Data access below
    private async IAsyncEnumerable<PerfIssueRegisterEntry> GetAllPerfIssuesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string filePath = _options.IssueFilePath;
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Issue repo file doesn't exist. Path: {filePath}", filePath);
            yield break;
        }

        using (Stream readingStream = File.OpenRead(_options.IssueFilePath))
        {
            await foreach (PerfIssueRegisterEntry? issue in JsonSerializer
                .DeserializeAsyncEnumerable<PerfIssueRegisterEntry>(readingStream, _jsonSerializerOptions, cancellationToken)
                .ConfigureAwait(false))
            {
                if (issue is null)
                {
                    continue;
                }
                yield return issue;
            }
        }
    }

    private async Task SaveAllPerfIssueAsync(IEnumerable<PerfIssueRegisterEntry> perfIssues, CancellationToken cancellationToken)
    {
        string tempFilePath = Path.GetTempFileName();
        using (Stream writingStream = File.OpenWrite(tempFilePath))
        {
            await JsonSerializer.SerializeAsync(writingStream, perfIssues, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        }
        File.Move(tempFilePath, _options.IssueFilePath, overwrite: true);
    }
}