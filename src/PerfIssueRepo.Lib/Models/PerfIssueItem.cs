using System.Globalization;

namespace PerfIssueRepo.Models;

public record PerfIssueItem : PerfIssue
{
    public string TypeCode { get; init; } = IssueType.Unknown;

    public string UniqueId => TypeCode + IssueId.ToString("D4", CultureInfo.InvariantCulture);
}