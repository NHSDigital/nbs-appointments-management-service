#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$environment
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$slotName = "preview"
$region = "uks"
$resourceGroup = "nbs-mya-rg-$environment-uks"

$webAppService = "nbs-mya-app-$environment-$region"
$functionAppServices = @("nbs-mya-func-$environment-$region", "nbs-mya-hlfunc-$environment-$region", "nbs-mya-sbfunc-$environment-$region", "nbs-mya-timerfunc-$environment-$region")

foreach ($functionApp in $functionAppServices) {
  Write-Host "Stopping slot '$slotName' for function app '$functionApp' in resource group '$resourceGroup'"
  az functionapp stop `
    --resource-group $resourceGroup `
    --name $functionApp `
    --slot $slotName
}

Write-Host "Stopping slot '$slotName' for web app '$webAppService' in resource group '$resourceGroup'"
az webapp stop `
  --name $webAppService `
  --resource-group $resourceGroup `
  --slot $slotName


