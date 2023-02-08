using System;

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
}