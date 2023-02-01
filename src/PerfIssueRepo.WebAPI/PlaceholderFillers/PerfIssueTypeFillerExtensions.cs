using Microsoft.Extensions.DependencyInjection.Extensions;
using PerfIssueRepo.Models;

namespace PerfIssueRepo.WebAPI.Services;

internal static class PerfIssueTypeFillerExtensions
{
    public static IServiceCollection TryAddPerfIssueTypeFillers(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.Unknown, p.GetRequiredService<IssueTypeCodeService>()));
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.OnCPU, p.GetRequiredService<IssueTypeCodeService>()));
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.Memory, p.GetRequiredService<IssueTypeCodeService>()));
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.DiskIO, p.GetRequiredService<IssueTypeCodeService>()));
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.NetworkIO, p.GetRequiredService<IssueTypeCodeService>()));
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.Thread, p.GetRequiredService<IssueTypeCodeService>()));
        services.AddTransient<IPlaceholderFiller>(p => new PerfIssueTypeFiller(IssueType.OffCPU, p.GetRequiredService<IssueTypeCodeService>()));

        return services;
    }
}