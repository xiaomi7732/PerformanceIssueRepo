using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OPI.Client;
using OPI.WebUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    string? defaultScope = builder.Configuration.GetSection("OPIOptions")["DefaultScope"];
    if (string.IsNullOrEmpty(defaultScope))
    {
        throw new InvalidOperationException("Default scope for the backend is required.");
    }
    options.ProviderOptions.DefaultAccessTokenScopes.Add(defaultScope);
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Backend URL is overwritten by options pattern.
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();
builder.Services.AddOPIClient<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CSVReaderFactory>();

await builder.Build().RunAsync();
