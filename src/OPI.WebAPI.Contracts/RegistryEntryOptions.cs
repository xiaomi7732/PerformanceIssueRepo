namespace OPI.WebAPI.Contracts;

public record RegistryEntryOptions
{
    public bool AllowsDuplicatedHelpDocs { get; init; } = false;
    public bool AllowsNewSubstitutes { get; set; } = false;

    public static RegistryEntryOptions Default { get; } = new RegistryEntryOptions();
}