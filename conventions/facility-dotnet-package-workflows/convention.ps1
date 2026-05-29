#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

# Copy each published workflow template into the target repository when it differs.
$workflowNames = @(
	'apply-repo-conventions.yml',
	'ci.yml',
	'copilot-setup-steps.yml'
)

# Ensure the target workflows directory exists before copying templates.
$targetDirectory = Join-Path (Get-Location) '.github' 'workflows'
[System.IO.Directory]::CreateDirectory($targetDirectory) | Out-Null

# Compare and copy workflow templates using stable UTF-8 output.
foreach ($workflowName in $workflowNames) {
	$sourcePath = Join-Path $PSScriptRoot 'files' $workflowName
	$targetPath = Join-Path $targetDirectory $workflowName
	$sourceContent = [System.IO.File]::ReadAllText($sourcePath)
	$targetContent = if (Test-Path -LiteralPath $targetPath -PathType Leaf) { [System.IO.File]::ReadAllText($targetPath) } else { $null }

	if ($sourceContent -cne $targetContent) {
		[System.IO.File]::WriteAllText($targetPath, $sourceContent, $utf8)
		Write-Host "Updated Facility workflow '.github/workflows/$workflowName'."
	}
}
