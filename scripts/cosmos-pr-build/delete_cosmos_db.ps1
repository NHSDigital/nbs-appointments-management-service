#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

$ShortCommitHash = $ENV:SHORT_COMMIT_HASH
$PRBuildResourceGroup = "nbs-appts-prbuild-rg-dev-uks"
$DevSubscription = "07748954-52d6-46ce-95e6-2701bfc715b4"
$CosmosAccountName = "nbs-appts-prbuild-$ShortCommitHash-cdb-dev-uks"

az cosmosdb delete `
    --name $CosmosAccountName `
    --resource-group $PRBuildResourceGroup `
    --subscription $DevSubscription `
    --yes `
    --no-wait

