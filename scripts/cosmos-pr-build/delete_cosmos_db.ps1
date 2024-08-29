#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$PrBuildId = $ENV:PR_BUILD_ID
$PRBuildResourceGroup = "nbs-appts-prbuild-rg-dev-uks"
$DevSubscription = "07748954-52d6-46ce-95e6-2701bfc715b4"
$CosmosAccountName = "nbs-appts-prbuild-$PrBuildId-cdb-dev-uks"

az cosmosdb delete `
    --name $CosmosAccountName `
    --resource-group $PRBuildResourceGroup `
    --subscription $DevSubscription `
    --yes `
    --no-wait

