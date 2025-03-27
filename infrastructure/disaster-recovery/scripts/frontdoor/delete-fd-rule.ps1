#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$profileName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

az afd rule-set delete `
  --resource-group $resourceGroup `
  --rule-set-name "MyaOverrideRuleSetUKW" `
  --profile-name $profileName `
  --yes