#!/usr/bin/env pwsh

param (
        [string][Parameter(Mandatory)]$resourceGroup,
        [string][Parameter(Mandatory)]$cosmosAccountName
)

$ResourceGroup = $resourceGroup
$CosmosAccountName = $cosmosAccountName

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$connectionString =
az cosmosdb keys list `
        --name $CosmosAccountName `
        --resource-group $ResourceGroup `
        --type connection-strings `
        --query "connectionStrings[?keyKind == 'Primary'].connectionString | [0]" `
        --output tsv

$NameValue=$connectionString -replace ";", "`n" | ConvertFrom-StringData
$cosmosEndpoint = $NameValue.AccountEndpoint
$cosmosPrimaryKey = $NameValue.AccountKey

Write-Host "##vso[task.setvariable variable=CosmosEndpoint]$cosmosEndpoint"
Write-Host "##vso[task.setvariable variable=CosmosToken]$cosmosPrimaryKey"