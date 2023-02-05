using System.Globalization;

namespace OPI.Core.Models;

public record PerfIssueItem : PerfIssue
{
    public string TypeCode { get; init; } = IssueType.Unknown;

    public string UniqueId => TypeCode + IssueId.ToString("D4", CultureInfo.InvariantCulture);
}