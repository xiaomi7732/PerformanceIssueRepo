using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
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

return 0;

