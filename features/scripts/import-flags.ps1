#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$appConfigName,
[string][Parameter(Mandatory)]$sourceFile,
[bool][Parameter(Mandatory)]$confirmChanges
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"

if ($confirmChanges) {
    az appconfig kv import `
        --name $appConfigName `
        --source file `
        --path $sourceFile `
        --format json `
        --yes
} else {
    az appconfig kv import `
        --name $appConfigName `
        --source file `
        --path $sourceFile `
        --format json `

    az appconfig kv import `
        --name $appConfigName `
        --source file `
        --path empty.json `
        --format json `
        --yes
}