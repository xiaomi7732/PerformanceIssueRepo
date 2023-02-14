using Azure.Core;

namespace SPEssentials.Identities;

public abstract class TokenCredential<T> : TokenCredential
{
    private TokenCredential? _innerTokenCredential;
    protected abstract TokenCredential CreateTokenCredential();

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => GetCredential().GetToken(requestContext, cancellationToken);

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => GetCredential().GetTokenAsync(requestContext, cancellationToken);

    private TokenCredential GetCredential()
    {
        if (_innerTokenCredential is null)
        {
            _innerTokenCredential = CreateTokenCredential();
        }
        return _innerTokenCredential;
    }
}