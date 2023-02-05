using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.DependencyInjection;

namespace OPI.Client;

public static class OPIClientExtensions
{
    public static IServiceCollection AddOPIClient(this IServiceCollection services, Uri baseAddress)
    {
        services.AddHttpClient<OPIClient>(opt =>
        {
            opt.BaseAddress = baseAddress;
            opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        return services;
    }
}