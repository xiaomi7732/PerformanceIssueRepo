using System.Collections.Generic;
using System.Linq;

namespace PerfIssueRepo.Models;

public record PerfIssueRegisterEntry : PerfIssue
{
    public PerfIssueRegisterEntry()
    {
    }

    public PerfIssueRegisterEntry(int newId, PerfIssue issue, IEnumerable<string> supportedTypes)
    {
        Id = newId;
        Title = issue.Title;
        Description = issue.Description;
        DocLink = issue.DocLink;
        Recommendation = issue.Recommendation;
        Rationale = issue.Rationale;
        SupportedTypes = supportedTypes;
    }

    public IEnumerable<string> SupportedTypes { get; init; } = Enumerable.Empty<string>();

    public bool IsActive { get; init; }
}