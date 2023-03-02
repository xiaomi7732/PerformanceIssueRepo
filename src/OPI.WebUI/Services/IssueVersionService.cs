using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using OPI.Client;
using OPI.Core.Models;

namespace OPI.WebUI.Services;

public sealed class IssueVersionService : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly Guid _cacheKey = Guid.Parse("98a02d1b-ce62-43b0-9289-9c1d6be45f0a");
    private readonly IAnonymousOPIClient _opiClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;

    private IEnumerable<string>? _availableVersions;

    public IssueVersionService(
        IAnonymousOPIClient opiClient,
        IMemoryCache memoryCache,
        ILogger<IssueVersionService> logger)
    {
        _opiClient = opiClient ?? throw new ArgumentNullException(nameof(opiClient));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the list of spec version a given issue is in.
    /// </summary>
    public async IAsyncEnumerable<string> GetSpecVersionsAsync(PerfIssue issue, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            if (issue.PermanentId is null || issue.PermanentId.Value == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(issue.PermanentId));
            }

            Guid key = issue.PermanentId.Value;

            Dictionary<string, IEnumerable<Guid>>? versionIdMapping = await GetVersionIdMappingAsync(cancellationToken);
            if (versionIdMapping is null)
            {
                yield break;
            }

            foreach (string version in versionIdMapping.Where(item => item.Value.Any(id => id == issue.PermanentId.Value)).Select(item => item.Key))
            {
                yield return version;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Dictionary<string, IEnumerable<Guid>>?> GetVersionIdMappingAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, IEnumerable<Guid>>? result = await _memoryCache.GetOrCreateAsync<Dictionary<string, IEnumerable<Guid>>>(_cacheKey, factory: async (entry) =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            Dictionary<string, IEnumerable<Guid>> dict = new Dictionary<string, IEnumerable<Guid>>();
            foreach (string version in await GetAvailableVersionsAsync(cancellationToken).ConfigureAwait(false))
            {
                IEnumerable<PerfIssueItem> items = await _opiClient.ListAllAsync(version, cancellationToken).ConfigureAwait(false);
                dict.Add(version, items.Where(item => item.PermanentId is not null).Select(item => item.PermanentId!.Value));
            }
            return dict;
        });

        return result;
    }

    private async Task<IEnumerable<string>> GetAvailableVersionsAsync(CancellationToken cancellationToken)
    {
        if (_availableVersions is null)
        {
            _availableVersions = await _opiClient.ListSpecVersionsAsync(cancellationToken);
        }
        return _availableVersions;
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}