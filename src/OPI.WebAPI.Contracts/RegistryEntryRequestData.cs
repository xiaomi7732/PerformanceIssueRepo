using OPI.Core.Models;

namespace OPI.WebAPI.Contracts;

public record RegistryEntryRequestData
{
    public PerfIssueRegisterEntry Data { get; set; } = default!;

    public RegistryEntryOptions Options { get; set; } = RegistryEntryOptions.Default;
}