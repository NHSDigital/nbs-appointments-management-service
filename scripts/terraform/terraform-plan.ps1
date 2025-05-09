#!/usr/bin/env pwsh
param (
        [string][Parameter(Mandatory)]$azureSubscriptionId,
        [string][Parameter(Mandatory)]$terraformEnvironmentFolderPath,
        [string][Parameter(Mandatory)]$buildNumber
)

$env:ARM_CLIENT_ID=$env:servicePrincipalId
$env:ARM_CLIENT_SECRET=$env:servicePrincipalKey
$env:ARM_TENANT_ID=$env:tenantId
$env:ARM_SUBSCRIPTION_ID=$azureSubscriptionId

Set-Location $terraformEnvironmentFolderPath

terraform init
terraform plan `
    -no-color `
    -input=false `
    -var="GOV_NOTIFY_API_KEY=$env:GOV_NOTIFY_API_KEY" `
    -var="SPLUNK_HEC_TOKEN=$env:SPLUNK_HEC_TOKEN" `
    -var="NHS_MAIL_CLIENT_SECRET=$env:AUTH_PROVIDER_CLIENT_SECRET" `
    -var="OKTA_CLIENT_SECRET=$env:OKTA_CLIENT_SECRET" `
    -var="OKTA_PRIVATE_KEY_KID=$env:OKTA_PRIVATE_KEY_KID" `
    -var="OKTA_PEM=$env:OKTA_PEM" `
    -var="BUILD_NUMBER=$buildNumber