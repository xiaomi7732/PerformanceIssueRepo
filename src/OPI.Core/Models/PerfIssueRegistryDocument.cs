using System.Collections.Generic;
using System.Linq;

namespace OPI.Core.Models;

/// <summary>
/// A data contract for perf issue registry document.
/// See https://github.com/xiaomi7732/PerformanceIssueRepo/blob/main/specs/registry/perf-issue.json for an example.
/// </summary>
public class PerfIssueRegistryDocument
{
    /// <summary>
    /// The performance issue items.
    /// </summary>
    public IEnumerable<PerfIssueItem> Items { get; init; } = Enumerable.Empty<PerfIssueItem>();
}