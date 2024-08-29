#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$PrBuildId = $ENV:PR_BUILD_ID
$PRBuildResourceGroup = "nbs-appts-prbuild-rg-dev-uks"
$CosmosAccountName = "nbs-appts-prbuild-$PrBuildId-cdb-dev-uks"

$connectionString =
    az cosmosdb keys list `
        --name $CosmosAccountName `
        --resource-group $PRBuildResourceGroup `
        --type connection-strings `
        --query "connectionStrings[?keyKind == 'Primary'].connectionString | [0]" `
        --output tsv

$NameValue=$connectionString -replace ";", "`n" | ConvertFrom-StringData
$cosmosEndpoint = $NameValue.AccountEndpoint
$cosmosPrimaryKey = $NameValue.AccountKey

Write-Host "##vso[task.setvariable variable=COSMOS_ENDPOINT]$cosmosEndpoint"
Write-Host "##vso[task.setvariable variable=COSMOS_TOKEN;issecret=true]$cosmosPrimaryKey"

#$Env:COSMOS_ENDPOINT = $cosmosEndpoint
#$Env:COSMOS_TOKEN = $cosmosPrimaryKey