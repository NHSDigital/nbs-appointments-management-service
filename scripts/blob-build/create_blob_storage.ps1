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
    --locations regionName=uksouth failoverPriority=0 `
    --sku Standard_LRS