using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace OPI.WebUI;

public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(
        IAccessTokenProvider provider, NavigationManager navigation) : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: new[] { "http://localhost:5041/" },
            scopes: new[] { "api://549847ed-4e8b-47e3-829d-5e1a381ec08f/.default" }
        );
    }
}