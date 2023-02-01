using PerfIssueRepo.Models;

namespace PerfIssueRepo.WebAPI.Services;

public interface IPlaceholderFiller
{
    bool CanFill(PerfIssueItem target);
    Task<PerfIssueItem> FillAsync(PerfIssueItem original, CancellationToken cancellationToken);
}