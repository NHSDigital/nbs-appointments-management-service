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
  Write-Host "Starting slot '$slotName' for function app '$functionApp' in resource group '$resourceGroup'"
  az functionapp start `
    --resource-group $resourceGroup `
    --name $functionApp `
    --slot $slotName
  if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to start swap slot '$slotName' for function app '$functionApp' in resource group '$resourceGroup'. If this is the first time the pipeline is ran, this might be because terraform apply has not yet created that slot."
    $LASTEXITCODE = 0
  }
}

Write-Host "Starting slot '$slotName' for web app '$webAppService' in resource group '$resourceGroup'"
az webapp start `
  --name $webAppService `
  --resource-group $resourceGroup `
  --slot $slotName
if ($LASTEXITCODE -ne 0) {
  Write-Warning "Failed to start swap slot '$slotName' for function app '$functionApp' in resource group '$resourceGroup'. If this is the first time the pipeline is ran, this might be because terraform apply has not yet created that slot."
  $LASTEXITCODE = 0
}

exit 0

