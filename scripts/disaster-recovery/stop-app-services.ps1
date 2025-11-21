#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$environment,
  [string][Parameter(Mandatory)]$region
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$webAppService = "nbs-mya-app-$environment-$region"
$functionAppServices = @("nbs-mya-func-$environment-$region", "nbs-mya-hlfunc-$environment-$region", "nbs-mya-sbfunc-$environment-$region", "nbs-mya-timerfunc-$environment-$region")


foreach ($functionApp in $functionAppServices) {
  Write-Host "Stopping function app '$functionApp' in resource group '$resourceGroup'"
  az functionapp stop `
    --resource-group $resourceGroup `
    --name $functionApp
  if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to stop function app '$functionApp' in resource group '$resourceGroup'. The DR process will continue anyway."
    $LASTEXITCODE = 0
  }
}

Write-Host "Stopping web app '$webAppService' in resource group '$resourceGroup'"
az webapp stop `
  --name $webAppService `
  --resource-group $resourceGroup
if ($LASTEXITCODE -ne 0) {
  Write-Warning "Failed to stop Web App: $webAppService. The DR process will continue anyway."
  $LASTEXITCODE = 0
}

exit 0