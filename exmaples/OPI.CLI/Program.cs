using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenPerformanceIssues.Client;
using PerfIssueRepo.Models;

IServiceCollection services = new ServiceCollection();

string exeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
IConfiguration configuration =
    new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(exeDir, "appsettings.jsonc"))
        .AddCommandLine(args)
        .Build();

string? endpoint = configuration["Endpoint"];
if (string.IsNullOrEmpty(endpoint))
{
    Console.WriteLine("Endpoint is not specified.");
    return -1;
}
Console.WriteLine("Using endpoint: {0}", endpoint);
services.AddOPIClient(new Uri(endpoint));

using (ServiceProvider provider = services.BuildServiceProvider())
{
    OPIClient client = provider.GetRequiredService<OPIClient>();

    string? specVersion = configuration["IssueSpecVersion"];
    if (!string.IsNullOrEmpty(specVersion))
    {
        Console.WriteLine("List all normalized issues with spec-version: {0}", specVersion);
        foreach (PerfIssueItem item in await client.ListAllAsync("1.0.0-alpha1", default))
        {
            Console.WriteLine(item);
        }
    }
    else
    {
        Console.WriteLine("Issue spec version is not provided.");
    }

    Console.WriteLine("List all registered entries:");
    foreach (PerfIssueRegisterEntry entry in await client.ListAllRegisteredAsync(default))
    {
        Console.WriteLine(entry);
    }

    Console.WriteLine("List all issue type Code - Value");
    foreach (KeyValuePair<string, string> issueType in await client.ListAllIssueTypes(default))
    {
        Console.WriteLine("{0} - {1}", issueType.Key, issueType.Value);
    }
}
return 0;