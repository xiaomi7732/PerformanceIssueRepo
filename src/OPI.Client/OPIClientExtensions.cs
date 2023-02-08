using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace OPI.Client;

public static class OPIClientExtensions
{
    public static IServiceCollection AddOPIClient(this IServiceCollection services, Uri baseAddress, string optionSectionName = "OPIOptions")
    {
        services.AddOptions<OPIClientOptions>().Configure<IConfiguration>((opt, configuration) =>
        {
            configuration.GetSection(optionSectionName).Bind(opt);
        });

        services.AddHttpClient<OPIClient>(opt =>
        {
            opt.BaseAddress = baseAddress;
            opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        services.AddTransient<GitHubClient>(p =>
        {
            return new GitHubClient(new Octokit.ProductHeaderValue("OPI.Client"));
        });

        return services;
    }
}