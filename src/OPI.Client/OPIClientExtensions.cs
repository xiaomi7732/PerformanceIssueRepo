using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Octokit;

namespace OPI.Client;

public static class OPIClientExtensions
{
    public static IServiceCollection AddOPIOptions(this IServiceCollection services, string optionSectionName = "OPIOptions")
    {
        services.AddOptions<OPIClientOptions>().Configure<IConfiguration>((opt, configuration) =>
        {
            configuration.GetSection(optionSectionName).Bind(opt);
        });

        return services;
    }

    public static IServiceCollection AddAnonymousOPIClient(this IServiceCollection services, string clientName, Uri? baseAddress = default)
    {
        IHttpClientBuilder httpClientBuilder = services.AddHttpClient<IAnonymousOPIClient, OPIClient>(clientName, opt =>
        {
            opt.BaseAddress = baseAddress;
            opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        services.TryAddTransient<IGitHubClient>(p =>
        {
            return new GitHubClient(new Octokit.ProductHeaderValue("OPI.Client"));
        });

        return services;
    }

    public static IServiceCollection AddAuthenticatedOPIClient(this IServiceCollection services, string clientName, Uri? baseAddress = default)
    {
        IHttpClientBuilder httpClientBuilder = services.AddHttpClient<IAuthorizedOPIClient, OPIClient>(clientName, opt =>
        {
            opt.BaseAddress = baseAddress;
            opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        services.TryAddTransient<IGitHubClient>(p =>
        {
            return new GitHubClient(new Octokit.ProductHeaderValue("OPI.Client"));
        });

        return services;
    }
}