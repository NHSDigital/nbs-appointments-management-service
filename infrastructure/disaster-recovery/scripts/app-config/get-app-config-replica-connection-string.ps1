#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$environment,
        [string][Parameter(Mandatory)]$resourceGroup
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$appConfigAccountName = "nbs-mya-config-$environment-uks"

$appConfigConnection = 
az appconfig replica show `
        --store-name $appConfigAccountName `
        --name replicaukw `
        --query "endpoint" `
        --output tsv `

Write-Host "##vso[task.setvariable variable=APP_CONFIG_CONNECTION;issecret=true]$appConfigConnection"