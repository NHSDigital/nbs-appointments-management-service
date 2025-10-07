param (
    [string][Parameter(Mandatory)]$branchName,
    [string][Parameter(Mandatory)]$buildNumber
)

Write-Host `n"Extracting MYA version from:"
Write-Host `t"branch: $branchName"
Write-Host `t"with buildNumber: $buildNumber"

$versionInBranchName = "0.0.0"
if ($branchName.Contains("release/")) {
    $versionInBranchName = ($branchName -split "release/")[1].split("/")[0]
} 
elseif ($branchName.Contains("hotfix/"))
{
    $versionInBranchName = ($branchName -split "hotfix/")[1].split("/")[0]
} 
else {
    Write-Warning "Branch name $branchName does not contain 'release/' or 'hotfix/'. Assuming version to be 0.0.0."
}

$semverComponents = $versionInBranchName.split(".")
$major = if ($semverComponents[0]) { $semverComponents[0] } else { '0' }
$minor = if ($semverComponents[1]) { $semverComponents[1] } else { '0' }
$patch = if ($semverComponents[2]) { $semverComponents[2] } else { '0' }

$version = "$major.$minor.$patch.$buildNumber"
Write-Host "Extracted version: $version"
Write-Host "##vso[task.setvariable variable=buildNumber]$version"