#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$environment,
        [string][Parameter(Mandatory)]$resourceGroup,
        [string][Parameter(Mandatory)]$featureName,
        [string][Parameter(Mandatory)]$featureFlagPipelineVariableName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$appConfigAccountName = "nbs-mya-config-$environment-uks"

$featureFlag =
az appconfig feature show `
        --name $appConfigAccountName `
        --resource-group $ResourceGroup `
        --feature $featureName `
        --output tsv

## TODO: get the feature flag value from the above output
$featureFlagBooleanValue = $featureFlag.value

Write-Host "##vso[task.setvariable variable=$featureFlagPipelineVariableName;issecret=true]$featureFlagBooleanValue"