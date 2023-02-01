using PerfIssueRepo.Models;

namespace PerfIssueRepo.WebAPI.Services;

public class IssueItemFactory
{
    private readonly IEnumerable<IPlaceholderFiller> _issueTypePlaceholderFillers;

    public IssueItemFactory(
        IEnumerable<IPlaceholderFiller> issueTypePlaceholderFillers)
    {
        _issueTypePlaceholderFillers = issueTypePlaceholderFillers ?? throw new ArgumentNullException(nameof(issueTypePlaceholderFillers));
    }
    public async Task<PerfIssueItem> CreateAsync(PerfIssue spec, string issueTypeCode, CancellationToken cancellationToken)
    {
        PerfIssueItem newItem = new PerfIssueItem()
        {
            IssueId = spec.IssueId,
            Title = spec.Title,
            Description = spec.Description,
            DocURL = spec.DocURL,
            Recommendation = spec.Recommendation,
            Rationale = spec.Rationale,
            TypeCode = issueTypeCode,
        };

        foreach (PerfIssueTypeFiller filler in _issueTypePlaceholderFillers)
        {
            newItem = await filler.FillAsync(newItem, cancellationToken);
        }

        return newItem;
    }
}