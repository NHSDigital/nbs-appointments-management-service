#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$environment,
        [string][Parameter(Mandatory)]$resourceGroup
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$appConfigAccountName = "nbs-mya-config-$environment-uks-replicaukw"

$appConfigConnection =
az appconfig credential list `
        --name $appConfigAccountName `
        --resource-group $ResourceGroup `
        --query "[?name == 'Primary'].connectionString | [0]" `
        --output tsv

Write-Host "##vso[task.setvariable variable=APP_CONFIG_CONNECTION;issecret=true]$appConfigConnection"