#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$profileName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$originGroups = @("mya-http-api-ukw", "mya-high-load-api-ukw", "mya-web-ukw")

foreach($originGroup in $originGroups) {
    az afd origin-group delete `
    --resource-group $resourceGroup `
    --origin-group-name $originGroup `
    --profile-name $profileName `
    --yes
}
