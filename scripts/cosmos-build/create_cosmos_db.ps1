#!/usr/bin/env pwsh
param (
    [string][Parameter(Mandatory)]$resourceGroup,
    [string][Parameter(Mandatory)]$cosmosAccountName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$ResourceGroup = $resourceGroup
$CosmosAccountName = $cosmosAccountName
$DevSubscription = "07748954-52d6-46ce-95e6-2701bfc715b4"

az cosmosdb create `
    --name $CosmosAccountName `
    --resource-group $ResourceGroup `
    --subscription $DevSubscription `
    --locations regionName=uksouth failoverPriority=0 `
    --enable-automatic-failover=false `
    --backup-redundancy local `
    --capabilities EnableServerless `
    --default-consistency-level session `
    --backup-policy-type periodic `
    --backup-retention 8