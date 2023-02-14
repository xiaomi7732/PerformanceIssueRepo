using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OPI.WebAPI.RegistryStorage;

public static class RegistryStorageExtensions
{
    public static IServiceCollection AddRegistryStorage(this IServiceCollection services, string optionSectionName = "RegistryStorage")
    {
        services.AddOptions<RegistryStorageOptions>().Configure<IConfiguration>((opt, config)=>{
            config.GetSection(optionSectionName).Bind(opt);
        });

        services.TryAddSingleton<RegistryStorageCredential>();
        services.TryAddSingleton<IRegistryBlobClient, RegistryBlobClient>();

        return services;
    }
}