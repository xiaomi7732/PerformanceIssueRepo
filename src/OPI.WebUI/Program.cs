using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using OPI.Client;
using OPI.Core.Utilities;
using OPI.WebUI;
using OPI.WebUI.Services;

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
builder.Services.AddOPIOptions();

// Add json gen options
builder.Services.AddOptions<JsonGenOptions>().Configure<IConfiguration>((opt, configuration) =>
{
    configuration.GetSection("JsonGenOptions").Bind(opt);
});

builder.Services.AddSingleton<SubstituteExtractor>(_=> SubstituteExtractor.Instance);

// Register an authenticated client
string authOpiClientName = "opi-client-auth";
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();
builder.Services.AddHttpClient(authOpiClientName).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
builder.Services.AddAuthenticatedOPIClient(authOpiClientName);

// Register an anonymous client
string anonymousOpiClientName = "opi-client-anonymous";
builder.Services.AddAnonymousOPIClient(anonymousOpiClientName);

builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<IssueVersionService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CSVReaderFactory>();
builder.Services.AddScoped<SpecDataSyncService>();
builder.Services.AddScoped(typeof(CSVGen<>), typeof(CSVGen<>));

await builder.Build().RunAsync();
