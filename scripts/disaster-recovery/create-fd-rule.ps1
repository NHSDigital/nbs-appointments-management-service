#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$profileName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$ruleSetName = "MyaOverrideRuleSetUKW"
$ruleName = "AvailabilityQueryRouteOverride"

az afd rule-set create `
  --resource-group $resourceGroup `
  --rule-set-name "MyaOverrideRuleSetUKW" `
  --profile-name $profileName

az afd rule create `
    --resource-group $resourceGroup `
    --rule-set-name $ruleSetName `
    --profile-name $profileName `
    --rule-name $ruleName `
    --order 1 `
    --action-name 'RouteConfigurationOverride' `
    --forwarding-protocol MatchRequest `
    --origin-group "mya-high-load-api-ukw"

az afd rule condition add `
    --resource-group $resourceGroup `
    --rule-set-name $ruleSetName `
    --profile-name $profileName `
    --rule-name $ruleName `
    --match-variable UrlPath `
    --operator EndsWith `
    --match-values "/availability/query" `
    --transforms "Lowercase"