using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using OPI.Client;
using OPI.Core.Models;

string exeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;

IConfiguration configuration = BuildConfiguration();

string? endpoint = configuration["Endpoint"];
if (string.IsNullOrEmpty(endpoint))
{
    Console.WriteLine("Endpoint is not specified.");
    return -1;
}
Console.WriteLine("Using endpoint: {0}", endpoint);

// Get access token
PublicClientApplicationOptions options = new PublicClientApplicationOptions()
{
    Instance = "https://login.microsoftonline.com/",
    ClientId = "d0ff64df-c1ee-4200-a971-21d7c8647de2",  // opi-console
    TenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
};

string accessToken = await SignInUserAndGetTokenUsingMSAL(options, new[] { "api://5fba0a18-7118-4762-b8c5-f406244e164f/.default" });    // opi-webapi
Console.WriteLine("Access token: {0}", accessToken);

OPIClient client = OPIClientFactory.Instance.Create(new Uri(endpoint));

string? specVersion = configuration["IssueSpecVersion"];
if (!string.IsNullOrEmpty(specVersion))
{
    Console.WriteLine("List all normalized issues with spec-version: {0}", specVersion);
    foreach (PerfIssueItem item in await client.ListAllAsync(specVersion, default))
    {
        Console.WriteLine(item);
    }
}
else
{
    Console.WriteLine("Issue spec version is not provided.");
}

Console.WriteLine("List all registered entries:");
try
{
    foreach (PerfIssueRegisterEntry entry in await client.ListAllRegisteredAsync(default))
    {
        Console.WriteLine(entry);
    }
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    // Without access token, getting 401.
    Console.WriteLine(ex.ToString());
}

// Local methods:
Console.WriteLine("List available versions for issue registries");
foreach (string version in (await client.ListSpecVersionsAsync(default).ConfigureAwait(false)).OrderBy(t => t))
{
    Console.WriteLine(version);
}

Console.WriteLine("Get spec in json");
string result = await client.GetAllInJsonStringAsync("latest", default);
Console.WriteLine(result);

IConfiguration BuildConfiguration()
{
    IConfiguration configuration =
        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(exeDir, "appsettings.jsonc"))
            .AddCommandLine(args)
            .Build();
    return configuration;
}

async Task<string> SignInUserAndGetTokenUsingMSAL(PublicClientApplicationOptions configuration, string[] scopes)
{
    // build the AAd authority Url
    string authority = string.Concat(configuration.Instance, configuration.TenantId);

    // Initialize the MSAL library by building a public client application

    Console.WriteLine("For client id: {0}", configuration.ClientId);
    IPublicClientApplication application = PublicClientApplicationBuilder.Create(configuration.ClientId)
                                            .WithAuthority(authority)
                                            .WithRedirectUri("http://localhost")
                                            .Build();
    AuthenticationResult result;

    try
    {
        var accounts = await application.GetAccountsAsync();

        Console.WriteLine("Account number: {0}", accounts.Count());
        result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
    }
    catch (MsalUiRequiredException)
    {
        result = await application.AcquireTokenInteractive(scopes).ExecuteAsync();
    }
    return result.AccessToken;
}

return 0;

