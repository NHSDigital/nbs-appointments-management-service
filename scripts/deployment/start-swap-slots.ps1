#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$environment,
  [string][Parameter(Mandatory)]$region
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$webAppService = "nbs-mya-app-$environment-$region/preview"
$functionAppServices = @("nbs-mya-func-$environment-$region/preview", "nbs-mya-hlfunc-$environment-$region/preview", "nbs-mya-sbfunc-$environment-$region/preview", "nbs-mya-timerfunc-$environment-$region/preview")

foreach ($functionApp in $functionAppServices) {
  try {
    az functionapp start `
      --resource-group $resourceGroup `
      --name $functionApp
    Write-Host "Successfully started swap slot: $functionApp"
  } catch {
    Write-Warning "Failed to start swap slot: $functionApp. If this is the first time the pipeline is ran, this might be because terraform apply has not yet created that slot. Error: $_"
  }
}


try {
  az webapp start `
    --name $webAppService `
    --resource-group $resourceGroup
  Write-Host "Successfully started swap slot: $functionApp"
} catch {
  Write-Warning "Failed to start swap slot: $functionApp. If this is the first time the pipeline is ran, this might be because terraform apply has not yet created that slot. Error: $_"
}



