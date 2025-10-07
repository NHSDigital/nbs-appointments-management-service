$scriptPath = Join-Path $PSScriptRoot 'extract-version.ps1'

Describe "extract-version.ps1" {

    $testCases = @(
        @{ branchName = "refs/heads/release/1.2.3"; buildNumber = "45"; expectedVersion = "1.2.3.45" },
        @{ branchName = "refs/heads/hotfix/1.2.3"; buildNumber = "45"; expectedVersion = "1.2.3.45" },
        @{ branchName = "refs/heads/release/3.7"; buildNumber = "31"; expectedVersion = "3.7.0.31" },
        @{ branchName = "refs/heads/hotfix/9"; buildNumber = "31"; expectedVersion = "9.0.0.31" },
        @{ branchName = "release/"; buildNumber = "13"; expectedVersion = "0.0.0.13" },
        @{ branchName = "hotfix/"; buildNumber = "13"; expectedVersion = "0.0.0.13" },
        @{ branchName = "release/1.2.3/4.5.6"; buildNumber = "21"; expectedVersion = "1.2.3.21" },
        @{ branchName = "hotfix/1.2.3/4.5.6"; buildNumber = "21"; expectedVersion = "1.2.3.21" },
        @{ branchName = "some-branch/feature/some-feature"; buildNumber = "0"; expectedVersion = "0.0.0.0" }
    )

    It "Extracts <expectedVersion> from branch <branchName> and buildNumber <buildNumber>" -TestCases $testCases {
        param($branchName, $buildNumber, $expectedVersion)

        $result = powershell -File $scriptPath -branchName $branchName -buildNumber $buildNumber 2>&1
        $lines = $result -split "`r?`n"
        $lastLine = $lines[-1]

        $expectedLastLine = "##vso[task.setvariable variable=buildNumber]$expectedVersion"
        $lastLine | Should -Be $expectedLastLine
    }

    It "Writes a warning if the branch name does not contain 'release/' or 'hotfix/'" {
        $branchName = "some-branch/feature/some-feature"
        $buildNumber = "45"

        $result = powershell -File $scriptPath -branchName $branchName -buildNumber $buildNumber 2>&1
        $lines = $result -split "`r?`n"

        $lines | Should -Contain "WARNING: Branch name some-branch/feature/some-feature does not contain 'release/' or 'hotfix/'. Assuming version to be 0.0.0."
    }
}