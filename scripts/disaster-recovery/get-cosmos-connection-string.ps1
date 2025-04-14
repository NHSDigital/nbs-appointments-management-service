#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$environment,
        [string][Parameter(Mandatory)]$resourceGroup
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$cosmosAccountName = "nbs-mya-cdb-$environment-uks"

$connectionString =
az cosmosdb keys list `
        --name $cosmosAccountName `
        --resource-group $ResourceGroup `
        --type connection-strings `
        --query "connectionStrings[?keyKind == 'Primary'].connectionString | [0]" `
        --output tsv

$NameValue=$connectionString -replace ";", "`n" | ConvertFrom-StringData
$cosmosEndpoint = $NameValue.AccountEndpoint
$cosmosPrimaryKey = $NameValue.AccountKey

Write-Host "##vso[task.setvariable variable=COSMOS_ENDPOINT;issecret=true]$cosmosEndpoint"
Write-Host "##vso[task.setvariable variable=COSMOS_TOKEN;issecret=true]$cosmosPrimaryKey"