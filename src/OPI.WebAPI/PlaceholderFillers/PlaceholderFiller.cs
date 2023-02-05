using OPI.Core.Models;

namespace OPI.WebAPI.Services;

internal abstract class PlaceholderFiller : IPlaceholderFiller
{
    private readonly string _placeholder;

    public PlaceholderFiller(string placeholder)
    {
        if (string.IsNullOrEmpty(placeholder))
        {
            throw new ArgumentException($"'{nameof(placeholder)}' cannot be null or empty.", nameof(placeholder));
        }

        _placeholder = placeholder;
    }

    public abstract bool CanFill(PerfIssueItem target);

    public async Task<PerfIssueItem> FillAsync(PerfIssueItem original, CancellationToken cancellationToken)
    {
        if (CanFill(original))
        {
            return original with
            {
                Title = original.Title.Replace(_placeholder, EnsureNotNullOrEmpty(await ReplaceWithAsync(original.Title, cancellationToken)), StringComparison.OrdinalIgnoreCase),
                Description = original.Description.Replace(_placeholder, EnsureNotNullOrEmpty(await ReplaceWithAsync(original.Description, cancellationToken)), StringComparison.OrdinalIgnoreCase),
                Recommendation = original.Recommendation.Replace(_placeholder, EnsureNotNullOrEmpty(await ReplaceWithAsync(original.Recommendation, cancellationToken)), StringComparison.OrdinalIgnoreCase),
                Rationale = string.IsNullOrEmpty(original.Rationale) ? original.Rationale : original.Rationale.Replace(_placeholder, await ReplaceWithAsync(original.Rationale, cancellationToken)),
            };
        }
        return original;
    }

    protected abstract Task<string?> ReplaceWithAsync(string? source, CancellationToken cancellationToken);

    private string EnsureNotNullOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(input);
        }
        return input;
    }
}