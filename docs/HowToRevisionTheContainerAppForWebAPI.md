# Quick guide to revision the WebAPI - manually

This is how to use Azure CLI to reversion the app on a local box.
See [how to do it in GitHub Action](./HowActionReversionTheContainerAppForWebAPI.md).

## The setup

* The image to use is already in the registry.
* The container app is already setup to use system-assigned managed identity to pull images from the registry.
* On a local box, az cli is logged in and the account is set correctly.

We will revision the app manually.

## Steps

1. Setup environment variables:

    ```powershell
    $ContainerApp = 'opiapi'
    $ContainerAppResourceGroup = 'opi-dev-westus-002'
    $ImageTag = 'saarsdevhub.azurecr.io/opiapi:b6fe51cb6d11dc41848361aed204ba059bf332f0'
    ```

1. Update the container app

    ```powershell
    az containerapp update -n $ContainerApp -g $ContainerAppResourceGroup --image "$ImageTag"
    ```

    Output:

    ```powershell
    The behavior of this command has been altered by the following extension: containerapp
    {
        "id": "/subscriptions/bbe41737-1ade-44df-8e33-217f11b8b452/resourceGroups/opi-dev-westus-002/providers/Microsoft.App/containerapps/opiapi",
        "identity": {
            "principalId": "82bda0ba-da8b-4a62-9b4a-b03c96a7e8d3",
            "tenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
            "type": "SystemAssigned, UserAssigned",
            "userAssignedIdentities": {
            "/subscriptions/bbe41737-1ade-44df-8e33-217f11b8b452/resourcegroups/opi-dev-westus-storage-001/providers/Microsoft.ManagedIdentity/userAssignedIdentities/opi-dev-webapi-identity": {
                "clientId": "8cab1b9b-5de9-4b52-bf0d-9f40b63644a4",
                "principalId": "8239bba9-9e6e-4ac3-9e47-eb51f2f5790d"
            }
            }
        },
        "location": "West US",
        "name": "opiapi",
        "properties": {
            "configuration": {
            "activeRevisionsMode": "Single",
            "dapr": null,
            "identitySettings": [],
            "ingress": {
                "additionalPortMappings": null,
                "allowInsecure": false,
                "clientCertificateMode": null,
                "corsPolicy": null,
                "customDomains": null,
                "exposedPort": 0,
                "external": true,
                "fqdn": "opiapi.delightfulrock-a140dca6.westus.azurecontainerapps.io",
                "ipSecurityRestrictions": null,
                "stickySessions": null,
                "targetPort": 80,
                "targetPortHttpScheme": null,
                "traffic": [
                {
                    "latestRevision": true,
                    "weight": 100
                }
                ],
                "transport": "Auto"
            },
            "maxInactiveRevisions": null,
            "registries": [
                {
                "identity": "",
                "passwordSecretRef": "opiacrazurecrio-opiacr",
                "server": "opiacr.azurecr.io",
                "username": "opiacr"
                },
                {
                "identity": "system",
                "passwordSecretRef": "",
                "server": "saarsdevhub.azurecr.io",
                "username": ""
                }
            ],
            "runtime": null,
            "secrets": [
                {
                "name": "saarsdevhubazurecrio-opiapi-pull-token"
                },
                {
                "name": "saarsdevhubazurecrio-saarsdevhub"
                },
                {
                "name": "opiacrazurecrio-opiacr"
                }
            ],
            "service": null
            },
            "customDomainVerificationId": "3A4387FACE065BFD1461EDAB5B0BA1D56DA85BD8084D87F00F27DFF94BE9477B",
            "delegatedIdentities": [],
            "environmentId": "/subscriptions/bbe41737-1ade-44df-8e33-217f11b8b452/resourceGroups/opi-dev-westus-002/providers/Microsoft.App/managedEnvironments/opi-containerapps-env",
            "eventStreamEndpoint": "https://westus.azurecontainerapps.dev/subscriptions/bbe41737-1ade-44df-8e33-217f11b8b452/resourceGroups/opi-dev-westus-002/containerApps/opiapi/eventstream",
            "latestReadyRevisionName": "opiapi--6frlht4",
            "latestRevisionFqdn": "opiapi--kp87h4t.delightfulrock-a140dca6.westus.azurecontainerapps.io",
            "latestRevisionName": "opiapi--kp87h4t",
            "managedEnvironmentId": "/subscriptions/bbe41737-1ade-44df-8e33-217f11b8b452/resourceGroups/opi-dev-westus-002/providers/Microsoft.App/managedEnvironments/opi-containerapps-env",
            "outboundIpAddresses": [
            "65.52.115.219"
            ],
            "patchingMode": "Automatic",
            "provisioningState": "Succeeded",
            "runningStatus": "Running",
            "template": {
            "containers": [
                {
                "image": "saarsdevhub.azurecr.io/opiapi:b6fe51cb6d11dc41848361aed204ba059bf332f0",
                "imageType": "ContainerImage",
                "name": "opiapi",
                "probes": [],
                "resources": {
                    "cpu": 0.5,
                    "ephemeralStorage": "2Gi",
                    "memory": "1Gi"
                }
                }
            ],
            "initContainers": null,
            "revisionSuffix": "",
            "scale": {
                "maxReplicas": 1,
                "minReplicas": 0,
                "rules": null
            },
            "serviceBinds": null,
            "terminationGracePeriodSeconds": null,
            "volumes": null
            },
            "workloadProfileName": null
        },
        "resourceGroup": "opi-dev-westus-002",
        "systemData": {
            "createdAt": "2023-02-28T22:08:00.2941229",
            "createdBy": "saars@microsoft.com",
            "createdByType": "User",
            "lastModifiedAt": "2024-11-05T23:28:04.0340646",
            "lastModifiedBy": "saars@microsoft.com",
            "lastModifiedByType": "User"
        },
        "type": "Microsoft.App/containerApps"
    }
    ```
