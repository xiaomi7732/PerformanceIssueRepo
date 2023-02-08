namespace OPI.Core.Models;

public record PerfIssueItem : PerfIssue
{
    public PerfIssueItem()
    {

    }

    public PerfIssueItem(PerfIssue spec)
    {
        PermanentId = spec.PermanentId;
        Title = spec.Title;
        Description = spec.Description;
        DocURL = spec.DocURL;
        Recommendation = spec.Recommendation;
        Rationale = spec.Rationale;
        LegacyId = spec.LegacyId;
    }
}