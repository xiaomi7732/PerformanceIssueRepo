using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OPI.Client;
using OPI.WebUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(@"api://549847ed-4e8b-47e3-829d-5e1a381ec08f/.default");
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Backend URL is overwritten by options pattern.
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();
builder.Services.AddOPIClient<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CSVReaderFactory>();

await builder.Build().RunAsync();
