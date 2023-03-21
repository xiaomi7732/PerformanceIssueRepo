using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using OPI.Core.Models;
using OPI.WebAPI.Contracts;

namespace OPI.Client;

public class OPIClient : IAuthorizedOPIClient
{
    private readonly HttpClient _httpClient;
    private readonly IGitHubClient? _gitHubClient;
    private readonly ILogger _logger;
    private readonly OPIClientOptions _clientOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public OPIClient(
        HttpClient httpClient,
        IGitHubClient? gitHubClient,
        IOptions<OPIClientOptions> clientOptions,
        ILogger<OPIClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gitHubClient = gitHubClient;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _clientOptions = clientOptions?.Value ?? throw new ArgumentNullException(nameof(clientOptions));

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = _clientOptions.BaseUri;
        }
        _logger.LogInformation("OPI Backend URI: {backend}", _httpClient.BaseAddress);

        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    /// <summary>
    /// Gets the base address of the http requests.
    /// </summary>
    public Uri? BaseAddress => _httpClient.BaseAddress;

    /// <summary>
    /// List all the performance issues of a given spec version.
    /// When latest is used, generate the issue list by the latest items in the registry.
    /// </summary>
    public Task<IEnumerable<PerfIssueItem>> ListAllAsync(string version, CancellationToken cancellationToken)
    {
        string path = $"issues?spec-version={version}";
        return ListAllAsync<PerfIssueItem>(path, cancellationToken);
    }

    /// <summary>
    /// Gets the perf issue item by its permanent id and the spec version.
    /// </summary>
    public async Task<PerfIssueItem?> GetPerfIssueItem(Guid issueId, string version, CancellationToken cancellationToken)
    {
        string path = $"Issues/{issueId}?spec-version={version}";

        PerfIssueItem? result = await _httpClient.GetFromJsonAsync<PerfIssueItem>(path, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// List all spec versions tagged on GitHub.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> ListSpecVersionsAsync(CancellationToken cancellationToken)
    {
        if (_gitHubClient is null)
        {
            throw new InvalidOperationException("GitHub client is missing. Please supply the github client first.");
        }

        return (await _gitHubClient.Repository.GetAllTags(
            _clientOptions.SpecRepositoryOwner,
            _clientOptions.SpecRepositoryName).ConfigureAwait(false))
            .Select(
                tag => tag.Name
            );
    }

    /// <summary>
    /// Gets all issues in form of Json string.
    /// </summary>
    public async Task<string> GetAllInJsonStringAsync(string version, CancellationToken cancellationToken)
    {
        string path = $"issues?spec-version={version}";
        return await _httpClient.GetStringAsync(path, cancellationToken).ConfigureAwait(false);
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
    /// Update a performance issue entry
    /// </summary>
    public async Task<PerfIssueRegisterEntry?> UpdateEntryAsync(RegistryEntryRequestData target, CancellationToken cancellationToken)
    {
        string path = "registry";
        JsonContent body = JsonContent.Create<RegistryEntryRequestData>(target, new MediaTypeHeaderValue(MediaTypeNames.Application.Json), _jsonSerializerOptions);
        HttpResponseMessage response = await _httpClient.PutAsync(path, body).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using (Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
        {
            PerfIssueRegisterEntry? result = await JsonSerializer.DeserializeAsync<PerfIssueRegisterEntry>(stream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }

    /// <summary>
    /// Registers a new entry
    /// </summary>
    /// <param name="newEntry"></param>
    public async Task<PerfIssueRegisterEntry> RegisterAsync(RegistryEntryRequestData newEntry, CancellationToken cancellationToken)
    {
        string path = "registry";
        JsonContent body = JsonContent.Create<RegistryEntryRequestData>(newEntry, new MediaTypeHeaderValue(MediaTypeNames.Application.Json), _jsonSerializerOptions);
        HttpResponseMessage response = await _httpClient.PostAsync(path, body, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        PerfIssueRegisterEntry? result = await response.Content.ReadFromJsonAsync<PerfIssueRegisterEntry>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            throw new InvalidOperationException("Result object is expected, null returned.");
        }
        return result;
    }

    /// <summary>
    /// Deletes a registered entry
    /// </summary>
    public async Task<bool> DeleteAsync(PerfIssueRegisterEntry targetEntry, CancellationToken cancellationToken)
    {
        string path = $"registry/{targetEntry.PermanentId:d}";
        HttpResponseMessage response = await _httpClient.DeleteAsync(path, cancellationToken).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Extract substitutes from a given spec or latest.
    /// </summary>
    /// <param name="specVersion">The version of the spec.</param>
    public async Task<IEnumerable<string>> ExtractSubstitutes(string specVersion, CancellationToken cancellationToken)
    {
        string path = $"substitutes?spec-version={specVersion}";
        using (Stream input = await _httpClient.GetStreamAsync(path, cancellationToken))
        {
            IEnumerable<string>? result = await JsonSerializer.DeserializeAsync<IEnumerable<string>>(input, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }
        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Append access token for the http client
    /// </summary>
    /// <param name="accessToken"></param>
    public void UseAccessToken(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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
