#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
$script:utf8 = $utf8
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

# Define the Pester suite for the Facility package workflow convention.
Describe 'facility-dotnet-package-workflows convention' {
	BeforeAll {
		# Record shared test state for direct convention invocation in isolated repositories.
		$script:utf8 = [System.Text.UTF8Encoding]::new($false)
		$script:conventionScriptPath = Join-Path $PSScriptRoot 'convention.ps1'
		$script:workflowNames = @('apply-repo-conventions.yml', 'ci.yml', 'copilot-setup-steps.yml')

		# Invoke the workflow convention script from the specified temporary repository.
		function script:InvokeFacilityWorkflowConvention {
			param(
				[Parameter(Mandatory = $true)]
				[string] $TestDirectory
			)

			$inputPath = Join-Path $TestDirectory 'convention-input.json'
			[System.IO.File]::WriteAllText($inputPath, '{"settings":{}}', $script:utf8)
			try {
				Push-Location $TestDirectory
				try {
					& $script:conventionScriptPath $inputPath
				}
				finally {
					Pop-Location
				}
			}
			finally {
				Remove-Item -LiteralPath $inputPath -ErrorAction SilentlyContinue
			}
		}

		function script:NormalizeWorkflowCronMinutes {
			param(
				[Parameter(Mandatory = $true)]
				[string] $WorkflowContent
			)

			$pattern = '(?m)^(\s*(?:-\s*)?cron:\s*)(["'']?)([^"''\r\n]+)\2(\s*)$'
			return [System.Text.RegularExpressions.Regex]::Replace($WorkflowContent, $pattern, {
				param($match)

				$parts = $match.Groups[3].Value.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
				if ($parts.Count -lt 5) {
					return $match.Value
				}

				$parts[0] = '*'
				$cronExpression = $parts -join ' '
				return "{0}{1}{2}{1}{3}" -f $match.Groups[1].Value, $match.Groups[2].Value, $cronExpression, $match.Groups[4].Value
			})
		}

		function script:GetScheduledCronMinute {
			param(
				[Parameter(Mandatory = $true)]
				[string] $WorkflowContent
			)

			$match = [System.Text.RegularExpressions.Regex]::Match($WorkflowContent, '(?m)^\s*-\s*cron:\s*["'']?([^"''\r\n]+)')
			$match.Success | Should -Be $true
			$parts = $match.Groups[1].Value.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
			$parts.Count | Should -BeGreaterOrEqual 5
			return [int] $parts[0]
		}
	}

	It 'copies the published workflow templates and is idempotent' {
		# Set up an isolated repository directory for workflow generation.
		$testDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString('N'))

		try {
			[System.IO.Directory]::CreateDirectory($testDirectory) | Out-Null

			# Apply the convention and assert each workflow matches the published template.
			InvokeFacilityWorkflowConvention -TestDirectory $testDirectory
			foreach ($workflowName in $script:workflowNames) {
				$sourcePath = Join-Path $PSScriptRoot 'files' $workflowName
				$targetPath = Join-Path $testDirectory '.github' 'workflows' $workflowName
				(Test-Path -LiteralPath $targetPath) | Should -Be $true
				$sourceContent = Get-Content -LiteralPath $sourcePath -Raw
				$targetContent = Get-Content -LiteralPath $targetPath -Raw
				(NormalizeWorkflowCronMinutes -WorkflowContent $targetContent) | Should -Be (NormalizeWorkflowCronMinutes -WorkflowContent $sourceContent)

				if ($workflowName -eq 'apply-repo-conventions.yml') {
					$minute = GetScheduledCronMinute -WorkflowContent $targetContent
					$minute | Should -BeGreaterThan 0
					$minute | Should -BeLessThan 60
				}
			}

			# Re-run the convention and assert no workflow content changes.
			$before = Get-ChildItem -LiteralPath (Join-Path $testDirectory '.github' 'workflows') -File | Sort-Object Name | ForEach-Object { $_.Name, (Get-Content -LiteralPath $_.FullName -Raw) }
			InvokeFacilityWorkflowConvention -TestDirectory $testDirectory
			$after = Get-ChildItem -LiteralPath (Join-Path $testDirectory '.github' 'workflows') -File | Sort-Object Name | ForEach-Object { $_.Name, (Get-Content -LiteralPath $_.FullName -Raw) }
			$after | Should -Be $before
		}
		finally {
			# Remove the isolated repository after the test completes.
			Remove-Item -LiteralPath $testDirectory -Recurse -Force
		}
	}

	It 'uses the Facility Bot GitHub App token in the apply workflow' {
		# Read the published apply workflow and assert the app-token wiring is present.
		$content = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'files' 'apply-repo-conventions.yml') -Raw
		$content | Should -Match 'actions/create-github-app-token@v3'
		$content | Should -Match 'vars\.FACILITY_BOT_CLIENT_ID'
		$content | Should -Match 'secrets\.FACILITY_BOT_PRIVATE_KEY'
		$content | Should -Match 'facility-bot\[app\]'
	}
}
