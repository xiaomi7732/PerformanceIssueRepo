using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using SPEssentials.Identities;

namespace OPI.WebAPI.RegistryStorage;

internal sealed class RegistryStorageCredential : TokenCredential<RegistryBlobClient>
{
    private readonly RegistryStorageOptions _options;

    public RegistryStorageCredential(IOptions<RegistryStorageOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override TokenCredential CreateTokenCredential()
    {
        if (!Enum.TryParse<CredentialType>(_options.Credential, ignoreCase: true, out CredentialType credentialType))
        {
            throw new InvalidDataException($"Unrecognized credential type from options: {_options.Credential}");
        }

        switch (credentialType)
        {
            case CredentialType.AzureDefault:
                return new DefaultAzureCredential();
            case CredentialType.ManagedIdentity:
                return new ManagedIdentityCredential(_options.ClientId);
            case CredentialType.SNICert:
            default:
                throw new NotSupportedException($"Not supported credential type of {credentialType} for Registry Storage.");

        }
    }
}
