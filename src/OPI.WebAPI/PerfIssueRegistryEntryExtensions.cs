using OPI.Core.Models;

namespace OPI.WebAPI;

internal static class PerfIssueRegistryEntryExtensions
{
    public static PerfIssueRegisterEntry TrackUpdate(this PerfIssueRegisterEntry entry, IHttpContextAccessor httpContextAccessor)
    {
        return Track(httpContextAccessor, (userName, utcNow) =>
        {
            entry = entry with
            {
                LastModifiedAt = utcNow,
                LastModifiedBy = userName,
            };

            return entry;
        });
    }

    public static PerfIssueRegisterEntry TrackCreate(this PerfIssueRegisterEntry entry, IHttpContextAccessor httpContextAccessor)
        => Track(httpContextAccessor, (userName, utcNow) =>
        {
            entry = entry with
            {
                CreatedAt = utcNow,
                CreatedBy = userName,
                LastModifiedAt = utcNow,
                LastModifiedBy = userName,
            };
            return entry;
        });


    private static PerfIssueRegisterEntry Track(IHttpContextAccessor httpContextAccessor, Func<string, DateTime, PerfIssueRegisterEntry> tracker)
    {
        string? userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            throw new InvalidOperationException("User info doesn't exist on the http context.");
        }

        DateTime utcNow = DateTime.UtcNow;

        return tracker(userName, utcNow);
    }
}