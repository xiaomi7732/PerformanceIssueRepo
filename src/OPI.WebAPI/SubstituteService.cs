using OPI.Core.Models;
using OPI.Core.Utilities;

namespace OPI.WebAPI.Services;

public class SubstituteService
{
    private readonly SubstituteExtractor _substituteExtractor;

    public SubstituteService(SubstituteExtractor substituteExtractor)
    {
        _substituteExtractor = substituteExtractor ?? throw new ArgumentNullException(nameof(substituteExtractor));
    }

    public async Task<IEnumerable<string>> ListSubstitutesAsync(Func<CancellationToken, Task<IEnumerable<PerfIssue>>> perfIssueItemProvider, CancellationToken cancellationToken)
    {
        HashSet<string> substitutes = new HashSet<string>(StringComparer.Ordinal);
        foreach (PerfIssue item in await perfIssueItemProvider(cancellationToken).ConfigureAwait(false))
        {
            _substituteExtractor.ExtractSubstitutes(() => item.Title, substitutes);
            _substituteExtractor.ExtractSubstitutes(() => item.Description, substitutes);
            _substituteExtractor.ExtractSubstitutes(() => item.Recommendation, substitutes);
            _substituteExtractor.ExtractSubstitutes(() => item.Rationale, substitutes);
        }
        return substitutes;
    }
}