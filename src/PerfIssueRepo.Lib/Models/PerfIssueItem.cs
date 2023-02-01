namespace PerfIssueRepo.Models;

public record PerfIssueItem : PerfIssue
{
    public PerfIssueItem(PerfIssue issueSpec, string issueTypeCode)
    {
        Id = issueSpec.Id;
        Title = issueSpec.Title;
        Description = issueSpec.Description;
        DocLink = issueSpec.DocLink;
        Recommendation = issueSpec.Recommendation;
        Rationale = issueSpec.Rationale;
        Type = issueTypeCode;
    }

    public string Type { get; init; } = IssueType.Unknown;
}