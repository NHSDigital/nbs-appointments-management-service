#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$environment,
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$writeRegion,
  [string][Parameter(Mandatory)]$readRegion
)

$cosmosAccountName = "nbs-mya-cdb-$environment-uks"

$accountId=$(az cosmosdb show --resource-group $resourceGroup --name $cosmosAccountName --query id -o tsv)

az cosmosdb failover-priority-change `
    --ids $accountId `
    --failover-policies "$writeRegion=0" "$readRegion=1" `
    