#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$subscriptionId
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$storageAccountNames = @(az storage account list --resource-group $resourceGroup --query "[].name" --output tsv)

foreach ($storageAccountName in $storageAccountNames) {
  Write-Host("Deleting storage account " + $storageAccountName)
  az storage account delete `
      --name $storageAccountName `
      --resource-group $resourceGroup `
      --subscription $subscriptionId `
      --yes `
}
