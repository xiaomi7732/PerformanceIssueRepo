﻿using System.Text.Json;
using OPI.Core.Models;

namespace OPI.Client;
public class OPIClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public OPIClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public Task<IEnumerable<PerfIssueItem>> ListAllAsync(string version, CancellationToken cancellationToken)
    {
        string path = $"issues?spec-version={version}";
        return ListAllAsync<PerfIssueItem>(path, cancellationToken);
    }

    /// <summary>
    /// List all the registered issues.
    /// </summary>
    public Task<IEnumerable<PerfIssueRegisterEntry>> ListAllRegisteredAsync(CancellationToken cancellationToken)
        => ListAllAsync<PerfIssueRegisterEntry>("registry", cancellationToken);

    public async Task<PerfIssueRegisterEntry?> ToggleActivateAsync(int issueId, CancellationToken cancellationToken)
    {
        string path = $"registry/{issueId}";
        using (HttpResponseMessage responseMessage = await _httpClient.PatchAsync(path, content: null, cancellationToken))
        {
            responseMessage.EnsureSuccessStatusCode();
            PerfIssueRegisterEntry? result = null;
            using (Stream responseStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken))
            {
                result = await JsonSerializer.DeserializeAsync<PerfIssueRegisterEntry>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            }
            return result;
        }
    }

    /// <summary>
    /// List all the issue types.
    /// </summary>
    public async Task<Dictionary<string, string>> ListAllIssueTypes(CancellationToken cancellationToken)
    {
        string path = "issuetypes";
        using Stream stream = await _httpClient.GetStreamAsync(path).ConfigureAwait(false);
        Dictionary<string, string>? result = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return new Dictionary<string, string>();
        }
        return result;
    }

    /// <summary>
    /// Generally return an enumerable of items
    /// </summary>
    private async Task<IEnumerable<T>> ListAllAsync<T>(string path, CancellationToken cancellationToken)
    {
        using Stream stream = await _httpClient.GetStreamAsync(path).ConfigureAwait(false);
        IEnumerable<T>? result = await JsonSerializer.DeserializeAsync<IEnumerable<T>>(stream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Enumerable.Empty<T>();
        }
        return result;
    }
}