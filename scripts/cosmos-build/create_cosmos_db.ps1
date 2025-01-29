#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$resourceGroup,
[string][Parameter(Mandatory)]$cosmosAccountName,
[string][Parameter(Mandatory)]$subscriptionId
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

az cosmosdb create `
    --name $cosmosAccountName `
    --resource-group $resourceGroup `
    --subscription $subscriptionId `
    --locations regionName=uksouth failoverPriority=0 `
    --enable-automatic-failover=false `
    --backup-redundancy local `
    --capabilities EnableServerless `
    --default-consistency-level session `
    --backup-policy-type periodic `
    --backup-retention 8
