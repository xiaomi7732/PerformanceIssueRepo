using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PerfIssueRepo.Models;

namespace PerfIssueRepo.WebAPI.Services;

public class IssueService
{
    private readonly IssueServiceOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger _logger;

    public IssueService(
        IOptions<IssueServiceOptions> options,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<IssueService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Logic
    public async Task<PerfIssueItem> AddAnIssue(string issueType, PerfIssue issueSpec, CancellationToken cancellationToken)
    {
        int newId = await GetNextAvailableIssueIdAsync(cancellationToken).ConfigureAwait(false);
        PerfIssueRegisterEntry newItem = new PerfIssueRegisterEntry(newId, issueSpec, new string[] { issueType });
        return new PerfIssueItem(newItem, issueType);
    }

    public async Task<PerfIssueRegisterEntry?> GetRegisteredItem(int id, CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry item in GetAllIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item.Id == id)
            {
                return item;
            }
        }
        return null;
    }

    public async Task<PerfIssueRegisterEntry> RegisterNewIssueAsync(PerfIssueRegisterEntry registerEntry, CancellationToken cancellationToken)
    {
        int newId = await GetNextAvailableIssueIdAsync(cancellationToken).ConfigureAwait(false);

        // Any register entry shall have id of zero
        if (registerEntry.Id != 0)
        {
            throw new InvalidCastException("New perf issue registry entry should always have an id of zero.");
        }

        registerEntry = registerEntry with
        {
            Id = newId
        };

        List<PerfIssueRegisterEntry> result = new List<PerfIssueRegisterEntry>();
        await foreach (PerfIssueRegisterEntry item in GetAllIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(item);
        }
        result.Add(registerEntry);

        await SaveAllPerfIssueAsync(result, cancellationToken).ConfigureAwait(false);
        return registerEntry;
    }

    /// <summary>
    /// Update activation of an registry item. Return the updated PerfIssueRegistryEntry when updated. Null when item is not updated.
    /// Throws exception when item is not there.
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> FlipActivationAsync(int issueId, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> result = new List<PerfIssueRegisterEntry>();
        PerfIssueRegisterEntry? updated = null;

        await foreach (PerfIssueRegisterEntry entry in GetAllIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            // No hit;
            if (entry.Id != issueId)
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
            throw new IndexOutOfRangeException($"No issue entry found by id of {issueId}");
        }

        // Update happened
        await SaveAllPerfIssueAsync(result, cancellationToken).ConfigureAwait(false);
        return updated;
    }

    public async Task<bool> DeleteAnIssueAsync(int issueId, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> itemsToKeep = new List<PerfIssueRegisterEntry>();
        bool deleted = false;
        await foreach (PerfIssueRegisterEntry item in GetAllPerfIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item.Id == issueId)
            {
                deleted = true;
            }
            else
            {
                itemsToKeep.Add(item);
            }
        }
        return deleted;
    }

    /// <summary>
    /// Updates the perf issue registry entry. Returns the new result.
    /// Throws IndexOutOfRange exception when not found by id.
    /// </summary>
    public async Task<PerfIssueRegisterEntry> UpdateAsync(PerfIssueRegisterEntry newIssueRegistryItem, CancellationToken cancellationToken)
    {
        List<PerfIssueRegisterEntry> results = new List<PerfIssueRegisterEntry>();
        bool found = false;
        await foreach (PerfIssueRegisterEntry entry in GetAllIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (entry.Id != newIssueRegistryItem.Id)
            {
                results.Add(entry);
                continue;
            }

            results.Add(newIssueRegistryItem);
            found = true;
        }

        if (!found)
        {
            throw new IndexOutOfRangeException($"Can't find an entry by id: {newIssueRegistryItem.Id}");
        }

        await SaveAllPerfIssueAsync(results, cancellationToken).ConfigureAwait(false);
        return newIssueRegistryItem;
    }

    public IAsyncEnumerable<PerfIssueRegisterEntry> GetAllIssuesAsync(CancellationToken cancellationToken)
    {
        return GetAllPerfIssuesAsync(cancellationToken);
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

    private async Task<int> GetNextAvailableIssueIdAsync(CancellationToken cancellationToken)
    {
        int maxId = 0;
        await foreach (PerfIssue issue in GetAllIssuesAsync(cancellationToken))
        {
            if (issue.Id > maxId)
            {
                maxId = issue.Id;
            }
        }
        return maxId + 1;
    }
}