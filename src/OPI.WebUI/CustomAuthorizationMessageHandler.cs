using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using OPI.Client;

namespace OPI.WebUI;

public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    private readonly ILogger _logger;

    public CustomAuthorizationMessageHandler(
        IAccessTokenProvider provider, NavigationManager navigation, IOptions<OPIClientOptions> opiOptions, ILogger<CustomAuthorizationMessageHandler> logger) : base(provider, navigation)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (opiOptions?.Value is null)
        {
            throw new ArgumentNullException(nameof(opiOptions));
        }

        _logger.LogInformation("Configure handler for backend: {url}, using default scope of: {scope}",
            opiOptions.Value.BaseUri.AbsolutePath,
            opiOptions.Value.DefaultScope);

        ConfigureHandler(
            authorizedUrls: new[] { opiOptions.Value.BaseUri.AbsoluteUri },
            scopes: new[] { opiOptions.Value.DefaultScope }
        );
    }
}