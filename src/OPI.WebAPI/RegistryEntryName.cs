namespace OPI.WebAPI;

public class RegistryEntryName
{
    public const string RegistryEntryPrefix = "registry";

    public RegistryEntryName(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException("Registry entry id can't be empty.");
        }

        Value = FormattableString.Invariant($@"{RegistryEntryPrefix}/{id:D}.json");
    }
    public string Value { get; init; } = default!;
}