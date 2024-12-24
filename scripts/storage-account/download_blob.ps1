#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$outputFilePath,
  [string][Parameter(Mandatory)]$storageAccountName,
  [string][Parameter(Mandatory)]$containerName,
  [string][Parameter(Mandatory)]$blobName
)

az storage blob download `
    --account-name $storageAccountName `
    --container-name $containerName `
    --name $blobName `
    --file $outputFilePath
