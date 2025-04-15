#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$sourceFile
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$UKSouthPrimaryConnectionString =
az appconfig credential list `
        --name $appConfigAccountName `
        --resource-group $ResourceGroup `
        --query "[?name == 'Primary'].connectionString | [0]" `
        --output tsv

$UKWestReplicaEndpoint = 
az appconfig replica show `
        --store-name $appConfigAccountName `
        --name replicaukw `
        --query "endpoint" `
        --output tsv `

$appConfigConnection = $UKSouthPrimaryConnectionString -replace "Endpoint=[^;]+", "Endpoint=$UKWestReplicaEndpoint"

az appconfig kv import `
        --connection-string $appConfigConnection `
        --source file `
        --path $sourceFile `
        --format json `
        --yes
