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
  az functionapp stop `
    --resource-group $resourceGroup `
    --name $functionApp
}

az webapp stop `
  --name $webAppService `
  --resource-group $resourceGroup


