#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$environment
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$appConfigAccountName = "nbs-mya-config-$environment-uks"
$resourceGroup = "nbs-mya-rg-$environment-uks"

## Will be of format: Endpoint=some-url;Id=123;Secret=abc
## (where Id and Secret hold real values, and some-url is the endpoint)
$connectionString =
az appconfig credential list `
        --name $appConfigAccountName `
        --resource-group $ResourceGroup `
        --query "[?name == 'Primary Read Only'].connectionString | [0]" `
        --output tsv

Write-Host "##vso[task.setvariable variable=APP_CONFIG_CONNECTION;issecret=true]$connectionString"