#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$environment,
  [string][Parameter(Mandatory)]$region
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$webAppservice = "nbs-mya-app-$environment-$region"
$functionAppservices = @("nbs-mya-func-$environment-$region", "nbs-mya-hlfunc-$environment-$region", "nbs-mya-sbfunc-$environment-$region", "nbs-mya-timerfunc-$environment-$region")

# stop function app services
foreach ($functionApp in $functionAppservices) {
  az functionapp stop `
    --resource-group $resourceGroup `
    --name $functionApp
}

# stop web app service
az webapp stop `
  --name $webAppservice `
  --resource-group $resourceGroup


