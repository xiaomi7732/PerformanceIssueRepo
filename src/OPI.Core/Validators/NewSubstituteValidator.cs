using System;
using System.Collections.Generic;
using System.Linq;
using OPI.Core.Models;
using OPI.Core.Utilities;

namespace OPI.Core.Validators;

public record NewSubstituteValidator : ModelValidatorBase
{
    private readonly PerfIssue _target;
    private readonly IEnumerable<string> _existingSubstitutes;
    private readonly SubstituteExtractor _substituteExtractor;

    public NewSubstituteValidator(
        PerfIssue target,
        IEnumerable<string> existingSubstitutes,
        SubstituteExtractor? substituteExtractor = null)
    {
        _target = target ?? throw new System.ArgumentNullException(nameof(target));
        _existingSubstitutes = existingSubstitutes ?? throw new System.ArgumentNullException(nameof(existingSubstitutes));
        _substituteExtractor = substituteExtractor ?? SubstituteExtractor.Instance;
    }

    public NewSubstituteValidator(
        PerfIssue target,
        IEnumerable<PerfIssue> perfIssues,
        SubstituteExtractor? substituteExtractor = null)
    {

        _target = target ?? throw new System.ArgumentNullException(nameof(target));
        _substituteExtractor = substituteExtractor ?? SubstituteExtractor.Instance;
        if (perfIssues is null)
        {
            throw new ArgumentNullException(nameof(perfIssues));
        }
        
        HashSet<string> existingSubstitutes = new HashSet<string>(StringComparer.Ordinal);
        foreach(PerfIssue item in perfIssues)
        {
            _substituteExtractor.ExtractSubstitutes(() => item.Title, existingSubstitutes);
            _substituteExtractor.ExtractSubstitutes(() => item.Description, existingSubstitutes);
            _substituteExtractor.ExtractSubstitutes(() => item.Recommendation, existingSubstitutes);
            _substituteExtractor.ExtractSubstitutes(() => item.Rationale, existingSubstitutes);
        }
        _existingSubstitutes = existingSubstitutes;
    }

    protected override bool ValidateImp()
    {
        HashSet<string> substituteOnTarget = new HashSet<string>(StringComparer.Ordinal);
        int extracted = _substituteExtractor.ExtractSubstitutes(() => _target.Title, substituteOnTarget);
        extracted += _substituteExtractor.ExtractSubstitutes(() => _target.Description, substituteOnTarget);
        extracted += _substituteExtractor.ExtractSubstitutes(() => _target.Recommendation, substituteOnTarget);
        extracted += _substituteExtractor.ExtractSubstitutes(() => _target.Rationale, substituteOnTarget);
        // No substitute
        if (extracted == 0)
        {
            // No new substitute found. Validation passed.
            Reason = "No new substitute found.";
            return true;
        }

        // There's substitute but no existing ones, anything is new.
        if (_existingSubstitutes is null || !_existingSubstitutes.Any())
        {
            Reason = $"New substitute found: {string.Join(",", extracted)}";
            return false;
        }

        // There's substitute in the target as well as existing ones, find out if there's anything new.
        IEnumerable<string> newOnes = substituteOnTarget.Except(_existingSubstitutes, StringComparer.Ordinal);
        if (newOnes is not null && newOnes.Any())
        {
            Reason = $"New substitute found: {string.Join(",", newOnes)}";
            return false;
        }

        Reason = "No new substitute is found.";
        return true;
    }
}