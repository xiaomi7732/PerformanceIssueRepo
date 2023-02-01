using System;

namespace PerfIssueRepo.Models;

public abstract record PerfIssue
{
    public int Id { get; init; } = 0;
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Uri? DocURL { get; init; } = default!;
    public string Recommendation { get; init; } = default!;
    public string? Rationale { get; init; } = default;
}