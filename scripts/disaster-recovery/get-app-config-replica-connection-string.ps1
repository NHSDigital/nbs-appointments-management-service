#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$environment,
        [string][Parameter(Mandatory)]$resourceGroup
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$appConfigAccountName = "nbs-mya-config-$environment-uks"

## Will be of format: Endpoint=some-url;Id=123;Secret=abc
## (where Id and Secret hold real values, and some-url is the UK South endpoint)
$normalUKSouthConnectionString =
az appconfig credential list `
        --name $appConfigAccountName `
        --resource-group $ResourceGroup `
        --query "[?name == 'Primary Read Only'].connectionString | [0]" `
        --output tsv
## az appconfig credential list --name nbs-mya-config-stag-uks --resource-group nbs-mya-rg-stag-uks --query "[?name == 'Primary Read Only'].connectionString | [0]" --output tsv

$UKWestReplicaEndpoint = 
az appconfig replica show `
        --store-name $appConfigAccountName `
        --name replicaukw `
        --query "endpoint" `
        --output tsv `
## az appconfig replica show --store-name nbs-mya-config-stag-uks --name replicaukw --query "endpoint" --output tsv

## The Replica and the Origin share Access Keys, so the Id and Secret acquired above are still valid.
## To derive the ConnectionString of the replica, we need to swap out the Endpoint of the credential with the replica's own
## TODO: This SHOULD be doable in one command either with az replica --show or az credential list. It seems utterly rediculous that it isn't
## If anyone can do this without resorting to string manipulation, please do so and update this script.
$appConfigConnection = $normalUKSouthConnectionString -replace "Endpoint=[^;]+", "Endpoint=$UKWestReplicaEndpoint"

Write-Host "##vso[task.setvariable variable=APP_CONFIG_CONNECTION;issecret=true]$appConfigConnection"