#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$appConfigName,
[string][Parameter(Mandatory)]$sourceFile
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"


az appconfig kv import `
        --name $appConfigName `
        --source file `
        --path $sourceFile `
        --format json `
        --yes
