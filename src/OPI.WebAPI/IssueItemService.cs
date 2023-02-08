using System.Text.Json;
using OPI.Core.Models;

namespace OPI.WebAPI.Services;

public class IssueItemService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public IssueItemService(
        IHttpClientFactory httpClientFactory,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
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
}