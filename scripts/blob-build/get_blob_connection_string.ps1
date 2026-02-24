#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$resourceGroup,
        [string][Parameter(Mandatory)]$blobAccountName
)

$ResourceGroup = $resourceGroup
$BlobAccountName = $blobAccountName

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$connectionString =
az storage account show-connection-string -g $ResourceGroup -n $BlobAccountName --output tsv

Write-Host "##vso[task.setvariable variable=BLOB_STORAGE_CONNECTION_STRING;issecret=true]$connectionString"