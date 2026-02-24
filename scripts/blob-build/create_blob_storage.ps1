#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$resourceGroup,
[string][Parameter(Mandatory)]$blobAccountName,
[string][Parameter(Mandatory)]$subscriptionId
)


az storage account create `
    --name $blobAccountName `
    --resource-group $resourceGroup `
    --subscription $subscriptionId `
    --location uksouth `
    --sku Standard_LRS `
    --min-tls-version TLS1_2