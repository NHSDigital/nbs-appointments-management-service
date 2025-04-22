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
  az functionapp start `
    --resource-group $resourceGroup `
    --name $functionApp
}

az webapp start `
  --name $webAppService `
  --resource-group $resourceGroup


