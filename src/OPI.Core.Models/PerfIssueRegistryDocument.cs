using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OPI.Core.Models;

/// <summary>
/// A data contract for perf issue registry document.
/// See https://github.com/xiaomi7732/PerformanceIssueRepo/blob/main/specs/registry/perf-issue.json for an example.
/// </summary>
public class PerfIssueRegistryDocument
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "https://raw.githubusercontent.com/xiaomi7732/PerformanceIssueRepo/main/specs/registry/schema.20230306.json";

    /// <summary>
    /// The performance issue items.
    /// </summary>
    public IEnumerable<PerfIssueItem> Items { get; init; } = Enumerable.Empty<PerfIssueItem>();
}