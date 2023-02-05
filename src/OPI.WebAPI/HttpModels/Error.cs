namespace OPI.WebAPI.HttpModels;

internal record Error
{
    public string Message { get; init; } = default!;
    public Uri HelpLink { get; init; } = default!;
}