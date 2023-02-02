using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PerfIssueRepo.Models;

namespace PerfIssueRepo.WebAPI.Services;

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

    public async Task<IEnumerable<PerfIssueItem>> ListByAsync(string version, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException($"'{nameof(version)}' cannot be null or empty.", nameof(version));
        }

        string url = $"perf-issue.{version}.json";
        HttpClient client = _httpClientFactory.CreateClient("issue-spec");
        IEnumerable<PerfIssueItem>? result = await client.GetFromJsonAsync<IEnumerable<PerfIssueItem>>(url, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Enumerable.Empty<PerfIssueItem>();
        }
        return result;
    }
}