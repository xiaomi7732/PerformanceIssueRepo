namespace OPI.WebAPI.Contracts;

public record RegistryEntryOptions
{
    public bool AllowsDuplicatedHelpDocs { get; init; } = false;

    public static RegistryEntryOptions Default { get; } = new RegistryEntryOptions();
}