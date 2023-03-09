using System;

namespace OPI.Core.Models;

public record PerfIssueRegisterEntry : PerfIssue, ITrackable
{
    /// <summary>
    /// Default constructor is required to keep deserialization work.
    /// </summary>
    public PerfIssueRegisterEntry()
    {
        IsActive = false;
    }

    /// <summary>
    /// Simply construct an object from perf issue spec.
    /// </summary>
    public PerfIssueRegisterEntry(PerfIssue issue)
    {
        LegacyId = issue.LegacyId;
        PermanentId = issue.PermanentId;
        Title = issue.Title;
        Description = issue.Description;
        DocURL = issue.DocURL;
        Recommendation = issue.Recommendation;
        Rationale = issue.Rationale;

        // Be explicit about the value of IsActive to be false.
        // It could be overwritten by the object initializer upon called.
        IsActive = false;
    }

    public bool IsActive { get; init; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastModifiedBy { get; set; }
}