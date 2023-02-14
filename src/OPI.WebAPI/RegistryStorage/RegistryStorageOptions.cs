using SPEssentials.Identities;

namespace OPI.WebAPI.RegistryStorage;

internal class RegistryStorageOptions : CredentialOptionsBase
{
    public Uri? BlobServiceUri { get; set; } = null;
    public string ContainerName { get; set; } = default!;
}