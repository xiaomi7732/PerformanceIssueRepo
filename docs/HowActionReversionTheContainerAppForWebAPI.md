# Setup Federate

This post discuss about setting up federation identity to managed identity, and use that to reversion the container app, using GitHub Actions.

## Steps

1. Setup the managed identity of the app to pull from ACR.
1. Create a user managed identity
1. In identity configuration, find the `federated credentials` configiration and add a federated crednetails with the following parameters:

    ```ini
    Issuer: https://token.actions.githubusercontent.com
    Organization: xiaomi7732 (or a real org)
    Repository: PerformanceIssueRepo
    Entity: Environment
    Environemnt: production # Case sensitive
    Audience: api://AzureADTokenExchange # Hard-coded for AAD
    ```

    That leads to this `Subject identifier`:

    > repo:xiaomi7732/PerformanceIssueRepo:environment:production

1. Create a role assignment so that it will have permission to reversion the container app.
    * I assigned contributor role to the resource group for the managed identity.

1. Setup GitHub Action Secrets, for example:

    ```shell
    AZURE_CLIENT_ID = 959164a5-c1dd-4345-8ae2-376592a296d0    # Client Id of the managed identity
    AZURE_SUBSCRIPTION_ID = bbe41737-1ade-44df-8e33-217f11b8b452 # Subscription Id of the managed identity
    AZURE_TENANT_ID = 72f988bf-86f1-41af-91ab-2d7cd011db47    # TenantID of the managed identity (You will find it in the Josn View)
    ACR_NAME = saarsdevhub
    ACR_ENDPOINT = saarsdevhub.azurecr.io
    ```

1. Update GitHub action:
    1. Add permissions:

        ```yaml
        permissions:
            id-token: write # This is required for requesting the JWT
            contents: read  # This is required for actions/checkout
        ```

    1. Add a step:

        ```yaml
        - name: 'Az CLI login'
          uses: azure/login@v2
            with:
              client-id: ${{ secrets.AZURE_CLIENT_ID }}
              tenant-id: ${{ secrets.AZURE_TENANT_ID }}
              subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
        ```

    See [deploy-webapi-acr.yml](./../.github/workflows/deploy-webapi-acr.yml) for a full example.

## Troubleshoot

Run the action and if there's error about the entity, correct it.

For example, when the entity was set wrong, you might see an error complaining the subject not exist:

```shell
Error: AADSTS700213: No matching federated identity record found for presented assertion subject 'repo:xiaomi7732/PerformanceIssueRepo:environment:production'. Please note that the matching is done using a case-sensitive comparison. Check your federated identity credential Subject, Audience and Issuer against the presented assertion. https://learn.microsoft.com/entra/workload-id/workload-identity-federation Trace ID: 96b2d810-a81f-48e2-b578-c80c38490401 Correlation ID: 27101bac-6cd2-414b-b03b-503066d3f49e Timestamp: 2024-11-06 20:09:44Z
```

## References

* [Use the Azure Login action with OpenID Connect](https://learn.microsoft.com/azure/developer/github/connect-from-azure-openid-connect)
* [](https://docs.github.com/actions/security-for-github-actions/security-hardening-your-deployments/configuring-openid-connect-in-azure)
* [Configure an app to trust an external identity provider](https://learn.microsoft.com/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp)