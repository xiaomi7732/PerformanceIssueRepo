using Azure.Identity;

namespace OPI.WebAPI.RegistryStorage;

internal class BlobContainerCreator : BackgroundService
{
    private readonly IRegistryBlobClient _registryBlobClient;
    private readonly ILogger _logger;

    public BlobContainerCreator(
        IRegistryBlobClient registryBlobClient,
        ILogger<BlobContainerCreator> logger
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registryBlobClient = registryBlobClient ?? throw new ArgumentNullException(nameof(registryBlobClient));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Ensure blob container exists");
            await _registryBlobClient.CreateContainerIfNotExistsAsync(stoppingToken).ConfigureAwait(false);
            _logger.LogInformation("Ensured blob container exists");
        }
        catch (CredentialUnavailableException ex)
        {
            _logger.LogError(ex, "Token credential does not exist. Is Managed Identity configured correctly?");
        }
    }
}