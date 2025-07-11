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

Write-Host "Creating container $tfstateContainerName in storage account $storageAccountName in resource group: $resourceGroupName"

# Newly created Sandbox environments come with an automatically created storage account 
# for Terraform state files, matching the storage account naming format above (e.g.myatfstnonproduks for pen).
# It lacks the container we need though, which is why we need this script to create it. 
# This script will create the container on the initial run, 
# and will not error on subsequent runs because --fail-if-exists defaults to false.
# However, for newly created Sandboxes this command will yield an auth error because 
# authentication via Shared Access Signatures (SAS) is disabled by default.
# Before the first run of this script, this needs enabling through the Azure Portal.
# TODO: Either find another way to authenticate, or find a way to change this default behaviour without manual steps.
az storage container create `
    --name $tfstateContainerName `
    --account-name $storageAccountName