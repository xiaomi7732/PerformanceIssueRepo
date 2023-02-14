namespace SPEssentials.Identities;

public class CredentialOptionsBase
{
    /// <summary>
    /// Gets or sets the Client id.
    /// </summary>
    /// <value></value>
    public string? ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The credential type. Default to AzureDefault. See <cref='OPI.Core.Identities.CredentialType' /> for other supported types.
    /// </summary>
    /// <remarks>
    /// Please keep the property name. In Azure Function, they will automatically match the settings with managed identity.
    /// </remarks>
    public string Credential { get; set; } = CredentialType.AzureDefault.ToString();
}