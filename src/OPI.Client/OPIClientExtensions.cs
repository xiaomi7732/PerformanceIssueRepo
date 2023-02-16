using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace OPI.Client;

public static class OPIClientExtensions
{
    public static IServiceCollection AddOPIClient<THandler>(this IServiceCollection services, Uri? baseAddress = default, string optionSectionName = "OPIOptions")
        where THandler : DelegatingHandler
    {
        services.AddOptions<OPIClientOptions>().Configure<IConfiguration>((opt, configuration) =>
        {
            configuration.GetSection(optionSectionName).Bind(opt);
        });

        IHttpClientBuilder httpClientBuilder = services.AddHttpClient<OPIClient>(opt =>
        {
            opt.BaseAddress = baseAddress;
            opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        httpClientBuilder.AddHttpMessageHandler<THandler>();

        services.AddTransient<IGitHubClient>(p =>
        {
            return new GitHubClient(new Octokit.ProductHeaderValue("OPI.Client"));
        });

        return services;
    }
}