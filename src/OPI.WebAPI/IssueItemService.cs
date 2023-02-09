using System.Text.Json;
using System.Text.RegularExpressions;
using OPI.Core.Models;

namespace OPI.WebAPI.Services;

public class IssueItemService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IssueRegistryService _issueRegistryService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public IssueItemService(
        IHttpClientFactory httpClientFactory,
        IssueRegistryService issueRegistryService,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _issueRegistryService = issueRegistryService ?? throw new ArgumentNullException(nameof(issueRegistryService));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public async Task<IEnumerable<string>> ListSubstitutesAsync(string version, CancellationToken cancellationToken)
    {
        HashSet<string> substitutes = new HashSet<string>(StringComparer.Ordinal);
        foreach (PerfIssueItem item in await GetAllAsync(version, cancellationToken).ConfigureAwait(false))
        {
            ExtractSubstitutes(() => item.Title, substitutes);
            ExtractSubstitutes(() => item.Description, substitutes);
            ExtractSubstitutes(() => item.Recommendation, substitutes);
            ExtractSubstitutes(() => item.Rationale, substitutes);
        }
        return substitutes;
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
            await foreach (PerfIssueItem item in _issueRegistryService.GetAllIssueItems(true, cancellationToken))
            {
                items.Add(item);
            }

            return items;
        }

        // Get from a published version
        // .../1.0.0-alpha/specs/registry/perf-issue.json
        string url = $"{version}/specs/registry/perf-issue.json";
        HttpClient client = _httpClientFactory.CreateClient("issue-spec");
        IEnumerable<PerfIssueItem>? result = await client.GetFromJsonAsync<IEnumerable<PerfIssueItem>>(url, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Enumerable.Empty<PerfIssueItem>();
        }
        return result;
    }

    private void ExtractSubstitutes(Func<string?> textSelector, HashSet<string> dest)
    {
        foreach (string sub in ExtractSubstitutes(textSelector()))
        {
            dest.Add(sub);
        }
    }

    private IEnumerable<string> ExtractSubstitutes(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        const string pattern = @"{(.*?)}";
        foreach (Match match in Regex.Matches(text, pattern, RegexOptions.Singleline, TimeSpan.FromSeconds(1)))
        {
            yield return match.Groups[1].Value;
        }
    }
}