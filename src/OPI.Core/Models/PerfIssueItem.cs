using System.Globalization;
using System.Text.Json.Serialization;

namespace OPI.Core.Models;

public record PerfIssueItem : PerfIssue
{
    public string Category { get; set; } = "CPU";

    public string UniqueId => TypeCode + IssueId.ToString("D4", CultureInfo.InvariantCulture);

    [JsonIgnore]
    public string TypeCode { get; init; } = IssueType.Unknown;
}