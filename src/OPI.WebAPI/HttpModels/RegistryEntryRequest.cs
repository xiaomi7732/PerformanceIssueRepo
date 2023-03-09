using OPI.Core.Models;

namespace OPI.WebAPI.HttpModels;

public record RegistryEntryRequest : PerfIssueRegisterEntry
{
    public bool AllowSameHelpLinkUri { get; init; } = false;
}