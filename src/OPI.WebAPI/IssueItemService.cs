using System.Text.Json;
using OPI.Core.Models;
using OPI.Core.Utilities;

namespace OPI.WebAPI.Services;

public class IssueItemService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IssueRegistryService _issueRegistryService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly SubstituteService _substituteService;
    private readonly ILogger _logger;

    public IssueItemService(
        IHttpClientFactory httpClientFactory,
        IssueRegistryService issueRegistryService,
        JsonSerializerOptions jsonSerializerOptions,
        SubstituteExtractor substituteExtractor,
        SubstituteService substituteService,
        ILogger<IssueItemService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _issueRegistryService = issueRegistryService ?? throw new ArgumentNullException(nameof(issueRegistryService));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        _substituteService = substituteService ?? throw new ArgumentNullException(nameof(substituteService));
    }

    public async Task<IEnumerable<string>> ListSubstitutesAsync(string version, CancellationToken cancellationToken)
    {
        HashSet<string> substitutes = new HashSet<string>(StringComparer.Ordinal);
        return await _substituteService.ListSubstitutesAsync(
            async (stoppingToken) => await GetAllAsync(version, stoppingToken),
            cancellationToken).ConfigureAwait(false);
    }

    public Task<IEnumerable<PerfIssueItem>> ListByAsync(string version, CancellationToken cancellationToken)
        => GetAllAsync(version, cancellationToken);

    public async Task<PerfIssueItem?> GetAsync(string version, Guid permanentId, CancellationToken cancellationToken)
        => (await GetAllAsync(version, cancellationToken).ConfigureAwait(false)).FirstOrDefault(item => item.PermanentId == permanentId);

    private async Task<IEnumerable<PerfIssueItem>> GetAllAsync(string version, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException($"'{nameof(version)}' cannot be null or empty.", nameof(version));
        }

        // Generate latest version
        if (string.Equals(version, "latest", StringComparison.OrdinalIgnoreCase))
        {
            List<PerfIssueItem> items = new List<PerfIssueItem>();
            await foreach (PerfIssueItem item in _issueRegistryService.GetAllIssueItemsAsync(true, cancellationToken))
            {
                items.Add(item);
            }

            return items;
        }

        // Get from a published version
        // .../1.0.0-alpha/specs/registry/perf-issue.json
        string url = $"{version}/specs/registry/perf-issue.json";
        HttpClient client = _httpClientFactory.CreateClient("issue-spec");

        IEnumerable<PerfIssueItem>? result = null;
        try
        {
            PerfIssueRegistryDocument? document = await client.GetFromJsonAsync<PerfIssueRegistryDocument>(url, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            result = document?.Items;
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "Not able to deserialize PerfIssueRegistryDocument. This should not happen for any version higher than 1.0.0-alpha7.");
        }

        // This is for backward compatibility for 1.0.0-alpha7 and any versions released before it.
        if (result is null || !result.Any())
        {
            _logger.LogInformation("Try get perf issue item collection without schema.");
            result = await client.GetFromJsonAsync<IEnumerable<PerfIssueItem>>(url, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        }

        if (result is null)
        {
            return Enumerable.Empty<PerfIssueItem>();
        }
        return result;
    }

}