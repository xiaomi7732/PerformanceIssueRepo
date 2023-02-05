using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OPI.Core.Models;

namespace OPI.WebAPI.Services;

public class IssueService
{
    private readonly IssueServiceOptions _options;
    private readonly IssueItemFactory _issueItemFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger _logger;

    public IssueService(
        IOptions<IssueServiceOptions> options,
        IssueItemFactory issueItemFactory,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<IssueService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _issueItemFactory = issueItemFactory ?? throw new ArgumentNullException(nameof(issueItemFactory));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Logic
    public async Task<PerfIssueItem> AddAnIssue(string issueType, PerfIssue issueSpec, CancellationToken cancellationToken)
    {
        int newId = await GetNextAvailableIssueIdAsync(cancellationToken).ConfigureAwait(false);
        PerfIssueRegisterEntry newItem = new PerfIssueRegisterEntry(newId, issueSpec, new string[] { issueType });
        return await _issueItemFactory.CreateAsync(newItem, issueType, cancellationToken);
    }

    public async Task<PerfIssueRegisterEntry?> GetRegisteredItem(int id, CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry item in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (item.IssueId == id)
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
        if (registerEntry.IssueId != 0)
        {
            throw new InvalidCastException("New perf issue registry entry should always have an id of zero.");
        }

        registerEntry = registerEntry with
        {
            IssueId = newId
        };

        List<PerfIssueRegisterEntry> result = new List<PerfIssueRegisterEntry>();
        await foreach (PerfIssueRegisterEntry item in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
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

        await foreach (PerfIssueRegisterEntry entry in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            // No hit;
            if (entry.IssueId != issueId)
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
            if (item.IssueId == issueId)
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
        await foreach (PerfIssueRegisterEntry entry in GetRegisteredIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (entry.IssueId != newIssueRegistryItem.IssueId)
            {
                results.Add(entry);
                continue;
            }

            results.Add(newIssueRegistryItem);
            found = true;
        }

        if (!found)
        {
            throw new IndexOutOfRangeException($"Can't find an entry by id: {newIssueRegistryItem.IssueId}");
        }

        await SaveAllPerfIssueAsync(results, cancellationToken).ConfigureAwait(false);
        return newIssueRegistryItem;
    }

    public IAsyncEnumerable<PerfIssueRegisterEntry> GetRegisteredIssuesAsync(CancellationToken cancellationToken)
    {
        return GetAllPerfIssuesAsync(cancellationToken);
    }

    public async IAsyncEnumerable<PerfIssueItem> GetAllIssueItems(bool? activeState, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PerfIssueRegisterEntry entry in GetAllPerfIssuesAsync(cancellationToken).ConfigureAwait(false))
        {
            if (activeState is null || entry.IsActive == activeState)
            {
                foreach (string typeCode in entry.SupportedTypes)
                {
                    PerfIssueItem newItem = await _issueItemFactory.CreateAsync(entry, typeCode, cancellationToken);
                    yield return newItem;
                }
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

    private async Task<int> GetNextAvailableIssueIdAsync(CancellationToken cancellationToken)
    {
        int maxId = 0;
        await foreach (PerfIssue issue in GetRegisteredIssuesAsync(cancellationToken))
        {
            if (issue.IssueId > maxId)
            {
                maxId = issue.IssueId;
            }
        }
        return maxId + 1;
    }
}