using System.Security.Claims;
using OPI.Core.Models;

namespace OPI.WebAPI;

internal static class PerfIssueRegistryEntryExtensions
{
    public static PerfIssueRegisterEntry TrackUpdate(this PerfIssueRegisterEntry entry, ITrackable existingTrackable, IHttpContextAccessor httpContextAccessor)
    {
        return Track(httpContextAccessor, (userName, utcNow) =>
        {
            entry = entry with
            {
                CreatedBy = existingTrackable.CreatedBy,
                CreatedAt = existingTrackable.CreatedAt,
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
        if (httpContextAccessor.HttpContext?.User?.Identity is not ClaimsIdentity identity)
        {
            throw new InvalidOperationException("Unsupported identity type. Expect it to be claims identity");
        }

        if (identity is null || !identity.IsAuthenticated)
        {
            throw new InvalidOperationException("The request is not authenticated.");
        }

        Claim? userNameClaim = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username");
        if (userNameClaim is null)
        {
            userNameClaim = identity.Claims.FirstOrDefault(c => c.Type == "name");
        }

        if (userNameClaim is null)
        {
            throw new InvalidOperationException("User info doesn't exist on the http context.");
        }

        string userName = userNameClaim.Value;
        DateTime utcNow = DateTime.UtcNow;

        return tracker(userName, utcNow);
    }
}