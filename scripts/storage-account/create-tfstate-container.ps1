#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$workloadName,

  [ValidateSet("dev", "int", "stag", "prod", "nonprod", "ukw")]
  [string][Parameter(Mandatory)]$workloadType,

  [ValidateSet("uks", "ukw")]
  [string][Parameter(Mandatory)]$workloadRegion,

  [string][Parameter(Mandatory)]$tfstateContainerName
)

## e.g. for the pen env: myatfstnonproduks
$storageAccountName = "${workloadName}tfst${workloadType}${workloadRegion}"

## e.g. for the pen env: nhsuk-mya-platform-rg-nonprod-uks
$resourceGroupName = "nhsuk-${workloadName}-platform-rg-${workloadType}-${workloadRegion}"

Write-Host "Creating storage account: $storageAccountName in resource group: $resourceGroupName"

az storage container create `
    --name $tfstateContainerName `
    --account-name $storageAccountName `
    --fail-on-exist false