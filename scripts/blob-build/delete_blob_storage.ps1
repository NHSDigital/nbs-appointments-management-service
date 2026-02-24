#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$resourceGroup,
[string][Parameter(Mandatory)]$blobAccountName,
[string][Parameter(Mandatory)]$subscriptionId
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

az storage account delete --subscription $subscriptionId --resource-group $resourceGroup --name $blobAccountName --yes