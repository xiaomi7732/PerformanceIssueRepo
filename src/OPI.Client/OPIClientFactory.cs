using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Octokit;

namespace OPI.Client;

public sealed class OPIClientFactory : IDisposable
{
    private HttpClient? _httpClient;
    private IGitHubClient? _gitHubClient;

    public static OPIClientFactory Instance { get; } = new OPIClientFactory();
    private OPIClientFactory()
    {
    }

    public OPIClient Create(Uri? backendBaseUri = null, string specRepoOwner = "xiaomi7732", string specRepositoryName = "PerformanceIssueRepo", ILoggerFactory? loggerFactory = null)
    {
        if (_httpClient is null)
        {
            _httpClient = new HttpClient();
        }

        if (_gitHubClient is null)
        {
            _gitHubClient = new GitHubClient(new Octokit.ProductHeaderValue("OPI.Client"));
        }

        OPIClientOptions options = new OPIClientOptions();
        if (backendBaseUri is not null)
        {
            options.BaseUri = backendBaseUri;
        }
        if (!string.IsNullOrEmpty(specRepoOwner))
        {
            options.SpecRepositoryOwner = specRepoOwner;
        }
        if (!string.IsNullOrEmpty(specRepositoryName))
        {
            options.SpecRepositoryName = specRepositoryName;
        }

        return new OPIClient(
            httpClient: _httpClient,
            gitHubClient: _gitHubClient,
            clientOptions: Options.Create(options),
            logger: loggerFactory?.CreateLogger<OPIClient>() ?? new NullLogger<OPIClient>());
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }
}