#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$ShortCommitHash = $ENV:SHORT_COMMIT_HASH
$PRBuildResourceGroup = "nbs-appts-prbuild-rg-dev-uks"
$CosmosAccountName = "nbs-appts-prbuild-$ShortCommitHash-cdb-dev-uks"

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
Write-Host "##vso[task.setvariable variable=COSMOS_TOKEN]$cosmosPrimaryKey"