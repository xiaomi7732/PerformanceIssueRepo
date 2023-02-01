using PerfIssueRepo.Models;

namespace PerfIssueRepo.WebAPI.Services;

internal class PerfIssueTypeFiller : PlaceholderFiller
{
    private readonly IssueTypeCodeService _codeService;
    private readonly string _issueCode;

    public PerfIssueTypeFiller(
        string issueCode,
        IssueTypeCodeService codeService
    ) : base("[issueType]")
    {
        if (string.IsNullOrEmpty(issueCode))
        {
            throw new ArgumentException($"'{nameof(issueCode)}' cannot be null or empty.", nameof(issueCode));
        }

        _issueCode = issueCode;
        _codeService = codeService ?? throw new ArgumentNullException(nameof(codeService));
    }

    public override bool CanFill(PerfIssueItem target)
    {
        return string.Equals(target.TypeCode, _issueCode, StringComparison.OrdinalIgnoreCase);
    }

    protected override async Task<string?> ReplaceWithAsync(string? source, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }
        string replaceWith = await _codeService.GetTypeStringAsync(_issueCode, cancellationToken);
        return replaceWith;
    }
}