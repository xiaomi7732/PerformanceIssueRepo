using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OPI.WebAPI;

public class SimpleHealthCheck : IHealthCheck
{
    private readonly IRegistryBlobClient _blobClient;
    private readonly ILogger _logger;

    public SimpleHealthCheck(
        IRegistryBlobClient blobClient,
        ILogger<SimpleHealthCheck> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _blobClient = blobClient ?? throw new ArgumentNullException(nameof(blobClient));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string reason = string.Empty;

        if (!await IsBlobStorageHealthyAsync(cancellationToken))
        {
            reason += "Can't access backend storage container.";
        }

        if (string.IsNullOrEmpty(reason))
        {
            return HealthCheckResult.Healthy("Healthy");
        }

        return HealthCheckResult.Unhealthy(reason);
    }

    private async Task<bool> IsBlobStorageHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _blobClient.CreateContainerIfNotExistsAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Can't access blob container.");
            return false;
        }
    }
}