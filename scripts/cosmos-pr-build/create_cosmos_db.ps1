#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$PrBuildId = $ENV:PR_BUILD_ID
$PRBuildResourceGroup = "nbs-appts-prbuild-rg-dev-uks"
$DevSubscription = "07748954-52d6-46ce-95e6-2701bfc715b4"
$CosmosAccountName = "nbs-appts-prbuild-$PrBuildId-cdb-dev-uks"

az cosmosdb create `
    --name $CosmosAccountName `
    --resource-group $PRBuildResourceGroup `
    --subscription $devSubscription `
    --locations regionName=uksouth failoverPriority=0 `
    --enable-automatic-failover=false `
    --backup-redundancy local `
    --capabilities EnableServerless `
    --default-consistency-level session `
    --backup-policy-type periodic `
    --backup-retention 8
