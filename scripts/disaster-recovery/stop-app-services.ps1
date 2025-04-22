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
  try {
    az functionapp stop `
      --resource-group $resourceGroup `
      --name $functionApp
    Write-Host "Successfully stopped Function App: $functionApp"
  } catch {
    Write-Warning "Failed to stop Function App: $functionApp. The DR process will continue anyway. Error: $_"
  }
}

# stop web app service
try {
  az webapp stop `
    --name $webAppService `
    --resource-group $resourceGroup
  Write-Host "Successfully stopped Web App: $webAppService"
} catch {
  Write-Warning "Failed to stop Web App: $webAppService. The DR process will continue anyway. Error: $_"
}


