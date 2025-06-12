#!/usr/bin/env pwsh
param (
  [string][Parameter(Mandatory)]$resourceGroup,
  [string][Parameter(Mandatory)]$environment,
  [string][Parameter(Mandatory)]$profileName
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

# These hostnames will be wrong if this is ever ran on perf, as perf follows a different naming convention.
# This isn't worth correcting, because we should never need to run this on perf and the addition of 
# -ukw on the end of each name will continue to ensure the names are unique.
$origins=@(
    @{originName="mya-http-api-ukw"; originGroupName="mya-http-api-ukw"; hostName="nbs-mya-func-$environment-ukw.azurewebsites.net"; originHostHeader="nbs-mya-func-$environment-ukw.azurewebsites.net"},
    @{originName="mya-high-load-api-ukw"; originGroupName="mya-high-load-api-ukw"; hostName="nbs-mya-hlfunc-$environment-ukw.azurewebsites.net"; originHostHeader="nbs-mya-hlfunc-$environment-ukw.azurewebsites.net"},
    @{originName="mya-web-ukw"; originGroupName="mya-web-ukw"; hostName="nbs-mya-app-$environment-ukw.azurewebsites.net"; originHostHeader="nbs-mya-app-$environment-ukw.azurewebsites.net"}
);

foreach ($origin in $origins) {
    az afd origin create `
        --resource-group $resourceGroup `
        --host-name  $origin.hostName`
        --profile-name $profileName `
        --origin-group-name $origin.originGroupName `
        --origin-name $origin.originName `
        --origin-host-header $origin.originHostHeader `
        --priority 1 `
        --weight 1000 `
        --enabled-state Enabled `
        --http-port 80 `
        --https-port 443
}