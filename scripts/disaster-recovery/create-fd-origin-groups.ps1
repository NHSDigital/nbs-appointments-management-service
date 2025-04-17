#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$profileName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$originGroups = @("mya-http-api-ukw", "mya-high-load-api-ukw", "mya-web-ukw")

foreach($originGroup in $originGroups) {
    az afd origin-group create `
    --resource-group $resourceGroup `
    --origin-group-name $originGroup `
    --profile-name $profileName `
    --sample-size 4 `
    --successful-samples-required 3 `
    --additional-latency-in-milliseconds 50
}
