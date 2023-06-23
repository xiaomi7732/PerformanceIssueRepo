namespace OPI.WebAPI.Contracts;

public record RegistryEntryOptions
{
    public bool AllowsDuplicatedHelpDocs { get; init; } = false;
    public bool AllowsNewSubstitutes { get; init; } = false;

    private static RegistryEntryOptions _default = new RegistryEntryOptions();
    public static RegistryEntryOptions Default { get; } = _default;
}