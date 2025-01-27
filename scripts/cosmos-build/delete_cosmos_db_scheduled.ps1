#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$subscriptionId
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$cosmosAccountNames = @(az cosmosdb list --resource-group $resourceGroup --query "[].name" --output tsv)

foreach ($cosmosAccountName in $cosmosAccountNames) {
  Write-Host("Deleting cosmos account " + $cosmosAccountName)
  az cosmosdb delete `
      --name $cosmosAccountName `
      --resource-group $resourceGroup `
      --subscription $subscriptionId `
      --yes `
      --no-wait `
}
