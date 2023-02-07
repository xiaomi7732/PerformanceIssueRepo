using OPI.Core.Models;

namespace OPI.WebAPI.Services;

public class IssueItemFactory
{
    private readonly IssueTypeCodeService _typeCodeService;
    private readonly IEnumerable<IPlaceholderFiller> _issueTypePlaceholderFillers;

    public IssueItemFactory(
        IssueTypeCodeService typeCodeService,
        IEnumerable<IPlaceholderFiller> issueTypePlaceholderFillers)
    {
        _typeCodeService = typeCodeService ?? throw new ArgumentNullException(nameof(typeCodeService));
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
            Category = await _typeCodeService.GetTypeStringAsync(issueTypeCode, cancellationToken).ConfigureAwait(false),
        };

        foreach (PerfIssueTypeFiller filler in _issueTypePlaceholderFillers)
        {
            newItem = await filler.FillAsync(newItem, cancellationToken);
        }

        return newItem;
    }
}