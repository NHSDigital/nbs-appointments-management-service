#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$environment,
[string][Parameter(Mandatory)]$sourceFile
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$UKSouthAppConfigAccountName = "nbs-mya-config-$environment-uks"
$UKSouthAppConfigResourceGroupName = "nbs-mya-rg-$environment-uks"

$UKSouthPrimaryConnectionString =
az appconfig credential list `
        --name $UKSouthAppConfigAccountName `
        --resource-group $UKSouthAppConfigResourceGroupName `
        --query "[?name == 'Primary'].connectionString | [0]" `
        --output tsv


$UKWestReplicaEndpoint = 
az appconfig replica show `
        --store-name $UKSouthAppConfigAccountName `
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
