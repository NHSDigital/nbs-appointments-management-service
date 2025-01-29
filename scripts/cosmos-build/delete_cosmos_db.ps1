#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$resourceGroup,
[string][Parameter(Mandatory)]$cosmosAccountName,
[string][Parameter(Mandatory)]$subscriptionId
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

az cosmosdb delete `
    --name $cosmosAccountName `
    --resource-group $resourceGroup `
    --subscription $subscriptionId `
    --yes `
    --no-wait
