#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$sourceFile
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

## Log Hello World
Write-Host "Entered import-flags-to-replica.ps1 script"
Write-Host "Source file: $sourceFile"

$UKSouthPrimaryConnectionString =
az appconfig credential list `
        --name $appConfigAccountName `
        --resource-group $ResourceGroup `
        --query "[?name == 'Primary'].connectionString | [0]" `
        --output tsv

Write-Host "Have fetched UK South Primary Connection String"

$UKWestReplicaEndpoint = 
az appconfig replica show `
        --store-name $appConfigAccountName `
        --name replicaukw `
        --query "endpoint" `
        --output tsv `

Write-Host "Have fetched UK West Replica Connection String"

$appConfigConnection = $UKSouthPrimaryConnectionString -replace "Endpoint=[^;]+", "Endpoint=$UKWestReplicaEndpoint"

Write-Host "Have derived the new conn string"

az appconfig kv import `
        --connection-string $appConfigConnection `
        --source file `
        --path $sourceFile `
        --format json `
        --yes
