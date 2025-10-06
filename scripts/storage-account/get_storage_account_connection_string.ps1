#!/usr/bin/env pwsh

param (
        [string][Parameter(Mandatory)]$resourceGroup,
        [string][Parameter(Mandatory)]$storageAccount
)

$ResourceGroup = $resourceGroup
$StorageAccount = $storageAccount

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$connectionString =
az storage account show-connection-string -g $ResourceGroup -n $StorageAccount --output tsv

Write-Host "##vso[task.setvariable variable=BlobStorageConnectionString;issecret=true]$connectionString"