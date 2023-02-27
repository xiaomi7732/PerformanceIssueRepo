using System.Runtime.CompilerServices;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Services;

public class IssueVersionService
{
    private readonly OPIClient _opiClient;
    private readonly ILogger _logger;
    private Dictionary<Guid, IEnumerable<string>> _cache;
    private IEnumerable<string>? _availableVersions;

    public IssueVersionService(
        OPIClient opiClient,
        ILogger<IssueVersionService> logger)
    {
        _opiClient = opiClient ?? throw new ArgumentNullException(nameof(opiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = new Dictionary<Guid, IEnumerable<string>>();
    }

    /// <summary>
    /// Gets the list of spec version a given issue is in.
    /// </summary>
    public async IAsyncEnumerable<string> GetSpecVersionsAsync(PerfIssue issue, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (issue.PermanentId is null || issue.PermanentId.Value == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(issue.PermanentId));
        }

        Guid key = issue.PermanentId.Value;

        // Already in cache
        if(_cache.ContainsKey(key))
        {
            foreach(string version in _cache[key])
            {
                yield return version;
            }
            yield break;
        }

        // Not in cache
        List<string> existInVersions = new List<string>();
        foreach(string specVersion in await GetAvailableVersionsAsync(cancellationToken).ConfigureAwait(false))
        {
            if((await _opiClient.ListAllAsync(specVersion, cancellationToken).ConfigureAwait(false)).Any(item => item.PermanentId == key))
            {
                existInVersions.Add(specVersion);
                yield return specVersion;
            }
        }
        _cache.TryAdd(key, existInVersions);
    }

    private async Task<IEnumerable<string>> GetAvailableVersionsAsync(CancellationToken cancellationToken)
    {
        if(_availableVersions is null)
        {
            _availableVersions = await _opiClient.ListSpecVersionsAsync(cancellationToken);
        }
        return _availableVersions;
    }
}