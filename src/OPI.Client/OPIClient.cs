using System.Text.Json;
using PerfIssueRepo.Models;

namespace OpenPerformanceIssues.Client;
public class OPIClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public OPIClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public async Task<IEnumerable<PerfIssueRegisterEntry>> ListAllAsync(CancellationToken cancellationToken)
    {
        string path = "registry";
        using Stream stream = await _httpClient.GetStreamAsync(path).ConfigureAwait(false);
        IEnumerable<PerfIssueRegisterEntry>? result = await JsonSerializer.DeserializeAsync<IEnumerable<PerfIssueRegisterEntry>>(stream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Enumerable.Empty<PerfIssueRegisterEntry>();
        }
        return result;
    }

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
}
