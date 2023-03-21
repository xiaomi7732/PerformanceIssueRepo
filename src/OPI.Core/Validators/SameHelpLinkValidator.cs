using System.Collections.Generic;
using System.Linq;
using OPI.Core.Models;

namespace OPI.Core.Validators;

public record SameHelpLinkValidator : ModelValidatorBase
{
    private readonly PerfIssue _target;
    private readonly IEnumerable<PerfIssue> _source;

    public SameHelpLinkValidator(PerfIssue target, IEnumerable<PerfIssue> source)
    {
        _target = target ?? throw new System.ArgumentNullException(nameof(target));
        _source = source ?? throw new System.ArgumentNullException(nameof(source));
    }

    protected override bool ValidateImp()
    {
        IEnumerable<PerfIssue> matched = _source.Where(item => !PerfIssue.IdComparer.Equals(item, _target)).Where(item => PerfIssue.HelpLinkComparer.Equals(_target, item));
        if (matched is null || !matched.Any())
        {
            return true;
        }
        Reason = $"Found item with duplicated HelpDoc of {_target.DocURL}. Target ids: {string.Join(",", matched.Select(item => item.PermanentId + ". Legacy id: " + item.LegacyId ?? "null"))}.";
        return false;
    }
}
