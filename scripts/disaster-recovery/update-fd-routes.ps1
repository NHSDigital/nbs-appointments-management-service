#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$env,
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$profileName,
  [string][Parameter(Mandatory)]$ruleSetName,
  [string][Parameter(Mandatory)]$apiOriginGroupName,
  [string][Parameter(Mandatory)]$webOriginGroupName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$endpointName = "nbs-mya"

# Update route to use ruleset
az afd route update `
  --resource-group $resourceGroup `
  --profile-name $profileName `
  --route-name "http-api-route-$env" `
  --endpoint-name $endpointName `
  --rule-sets $ruleSetName

# Update origin group on route
$routes=@(
    @{routeName="http-api-route-$env"; originGroupName=$apiOriginGroupName},
    @{routeName="web-route-$env"; originGroupName=$webOriginGroupName}
);

foreach ($route in $routes) {
    az afd route update `
        --resource-group $resourceGroup `
        --profile-name $profileName `
        --route-name $route.routeName `
        --endpoint-name $endpointName `
        --origin-group $route.originGroupName
}