using Microsoft.Extensions.DependencyInjection;
using OpenPerformanceIssues.Client;
using PerfIssueRepo.Models;

IServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddOPIClient(new Uri("https://opir-test.azurewebsites.net/"));

using (ServiceProvider provider = serviceCollection.BuildServiceProvider())
{
    OPIClient client = provider.GetRequiredService<OPIClient>();

    foreach (PerfIssueRegisterEntry entry in await client.ListAllAsync(default))
    {
        Console.WriteLine(entry.ToString());
    }
}