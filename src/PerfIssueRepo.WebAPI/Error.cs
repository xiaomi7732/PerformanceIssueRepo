namespace PerfIssueRepo.WebAPI;

internal record Error
{
    public string Message { get; init; } = default!;
    public Uri HelpLink { get; init; } = default!;
}