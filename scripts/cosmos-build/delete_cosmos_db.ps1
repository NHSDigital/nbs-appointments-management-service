#!/usr/bin/env pwsh
param (
    [string][Parameter(Mandatory)]$resourceGroup,
    [string][Parameter(Mandatory)]$cosmosAccountName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$ResourceGroup = $ResourceGroup
$CosmosAccountName = $CosmosAccountName
$DevSubscription = "07748954-52d6-46ce-95e6-2701bfc715b4"

az cosmosdb delete `
    --name $CosmosAccountName `
    --resource-group $ResourceGroup `
    --subscription $DevSubscription `
    --yes `
    --no-wait

