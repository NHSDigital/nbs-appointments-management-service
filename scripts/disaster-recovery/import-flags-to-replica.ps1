#!/usr/bin/env pwsh
param (
[string][Parameter(Mandatory)]$appConfigConnectionString,
[string][Parameter(Mandatory)]$sourceFile
)

$ErrorActionPreference = "Stop"
$DebugPreference = "Continue"


az appconfig kv import `
        --connection-string $appConfigConnectionString `
        --source file `
        --path $sourceFile `
        --format json `
        --yes
