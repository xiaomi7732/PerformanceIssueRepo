using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
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

    public async Task<PerfIssueRegisterEntry?> ToggleActivateAsync(Guid issueId, CancellationToken cancellationToken)
    {
        string path = $"registry/{issueId:D}";
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
    [Obsolete("This API is deprecated.", error: true)]
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
    /// Update a performance issue entry
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> UpdateEntryAsync(PerfIssueRegisterEntry target, CancellationToken cancellationToken)
    {
        string path = "registry";
        JsonContent body = JsonContent.Create<PerfIssueRegisterEntry>(target, new MediaTypeHeaderValue(MediaTypeNames.Application.Json), _jsonSerializerOptions);
        HttpResponseMessage response = await _httpClient.PutAsync(path, body).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using (Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
        {
            PerfIssueRegisterEntry? result = await JsonSerializer.DeserializeAsync<PerfIssueRegisterEntry>(stream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            return result;
        }
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
