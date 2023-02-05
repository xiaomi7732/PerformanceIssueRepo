using OPI.Core.Models;

namespace OPI.WebAPI.Services;

public interface IPlaceholderFiller
{
    bool CanFill(PerfIssueItem target);
    Task<PerfIssueItem> FillAsync(PerfIssueItem original, CancellationToken cancellationToken);
}