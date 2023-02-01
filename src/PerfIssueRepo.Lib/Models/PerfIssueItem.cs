using System.Globalization;

namespace PerfIssueRepo.Models;

public record PerfIssueItem : PerfIssue
{
    public PerfIssueItem(PerfIssue issueSpec, string issueTypeCode)
    {
        Id = issueSpec.Id;
        Title = issueSpec.Title;
        Description = issueSpec.Description;
        DocURL = issueSpec.DocURL;
        Recommendation = issueSpec.Recommendation;
        Rationale = issueSpec.Rationale;
        TypeCode = issueTypeCode;
    }

    public string TypeCode { get; init; } = IssueType.Unknown;

    public string DisplayId => TypeCode + Id.ToString("D3", CultureInfo.InvariantCulture);
}