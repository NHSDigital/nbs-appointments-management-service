#!/usr/bin/env pwsh

param (
        [string][Parameter(Mandatory)]$resourceGroup,
        [string][Parameter(Mandatory)]$storageAccount
)

$ResourceGroup = $resourceGroup
$StorageAccount = $storageAccount

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$connectionObject =
az storage account show-connection-string -g $ResourceGroup -n $StorageAccount

$connectionString = $connectionObject.ConnectionString

Write-Host "##vso[task.setvariable variable=BlobStorageConnectionString;issecret=true]$connectionString"