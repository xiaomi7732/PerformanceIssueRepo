using System;
using System.Collections.Generic;

namespace OPI.Core.Models;

public abstract record PerfIssue
{
    public Guid? PermanentId { get; set; } = Guid.Empty;
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Uri? DocURL { get; init; } = default!;
    public string Recommendation { get; init; } = default!;
    public string? Rationale { get; init; } = null;
    public string? LegacyId { get; init; } = null;

    public static PerfIssueIdComparer IdComparer { get; } = new PerfIssueIdComparer();
    public static PerfIssueHelpLinkComparer HelpLinkComparer { get; } = new PerfIssueHelpLinkComparer();
}

public class PerfIssueIdComparer : IEqualityComparer<PerfIssue>
{
    public bool Equals(PerfIssue x, PerfIssue y)
    {
        // Both have legacy id and they are the same, case insensitive.
        if (x.LegacyId is not null && y.LegacyId is not null)
        {
            return string.Equals(x.LegacyId, y.LegacyId, StringComparison.OrdinalIgnoreCase);
        }

        // Or both have Permanent id, and they are the same
        if (x.PermanentId is not null && y.PermanentId is not null)
        {
            return x.PermanentId.Value == y.PermanentId.Value;
        }

        // Otherwise, treat as not equal.
        return false;
    }

    public int GetHashCode(PerfIssue obj)
    {
        int hashCode = 32768;
        if (obj.LegacyId is not null)
        {
            hashCode = hashCode ^ obj.LegacyId.GetHashCode();
        }

        if (obj.PermanentId is not null)
        {
            hashCode = hashCode ^ obj.PermanentId.GetHashCode();
        }

        return hashCode;
    }
}

public class PerfIssueHelpLinkComparer : IEqualityComparer<PerfIssue>
{
    public bool Equals(PerfIssue x, PerfIssue y)
    {
        if (x is null || y is null || x.DocURL is null || y.DocURL is null)
        {
            return false;
        }
        return x.DocURL == y.DocURL;
    }

    public int GetHashCode(PerfIssue obj)
    {
        int hashCode = 32768;
        if (obj?.DocURL is null)
        {
            return hashCode;
        }
        return hashCode ^ obj.DocURL.GetHashCode();
    }
}